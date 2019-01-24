using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Workflow.Events.Args;
using Workflow.Extensions;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Notifications;
using Workflow.Services;
using Workflow.Services.Interfaces;

using TaskStatus = Workflow.Models.TaskStatus;
using TaskType = Workflow.Models.TaskType;

namespace Workflow.Processes
{
    public abstract class WorkflowProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IConfigService _configService;
        private readonly IGroupService _groupService;
        private readonly IInstancesService _instancesService;
        private readonly ITasksService _tasksService;

        private readonly Emailer _emailer;
        private readonly Utility _utility;
        private readonly WorkflowSettingsPoco _settings;

        protected WorkflowType Type { private get; set; }
        protected WorkflowInstancePoco Instance;

        public static event EventHandler<InstanceEventArgs> Created;
        public static event EventHandler<InstanceEventArgs> Cancelled;

        public IEnumerable<EventMessage> EventMessages;

        protected WorkflowProcess()
        {
            _configService = new ConfigService();
            _groupService = new GroupService();
            _instancesService = new InstancesService();
            _tasksService = new TasksService();

            _emailer = new Emailer();
            _utility = new Utility();

            ISettingsService settingsService = new SettingsService();
            _settings = settingsService.GetSettings();
        }

        # region Public methods
        /// <summary>
        /// Initiates a workflow process instance for this workflow type and persists it to the database.
        /// </summary>
        /// <param name="nodeId">The document that the workflow is for.</param>
        /// <param name="authorUserId">The author submitting the document to workflow</param>
        /// <param name="authorComment">Comments provided by the author.</param>
        /// <returns>The initiated workflow process instance entity.</returns>
        public WorkflowInstancePoco InitiateWorkflow(int nodeId, int authorUserId, string authorComment)
        {
            // create and persist the new workflow instance
            Instance = new WorkflowInstancePoco
            {
                NodeId = nodeId,
                AuthorUserId = authorUserId,
                AuthorComment = authorComment,
                Type = (int)Type,
                Guid = Guid.NewGuid()
            };

            Instance.SetScheduledDate();

            _instancesService.InsertInstance(Instance);

            // create the first task in the workflow and set the approval group
            WorkflowTaskPoco taskInstance = Instance.CreateApprovalTask();
            SetApprovalGroup(taskInstance);

            Created?.Invoke(this, new InstanceEventArgs(Instance));

            if (taskInstance.UserGroup == null)
            {
                string errorMessage = $"No approval flow set for document {nodeId} or any of its parent documents. Unable to initiate approval task.";

                Log.Error(errorMessage);
                throw new WorkflowException(errorMessage);
            }

            ApproveOrContinue(taskInstance);

            return Instance;
        }

        /// <summary>
        /// Manage resubmitting previously rejected workflow task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="userId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public WorkflowInstancePoco ResubmitWorkflow(WorkflowInstancePoco instance, int userId, string comment)
        {
            if (instance != null)
            {
                Instance = instance;

                if (Instance.WorkflowStatus != WorkflowStatus.Rejected) return Instance;

                // create a task to store the resubmission comment - this is the equivalent of the author comment on the instance
                var resubmitTask = new WorkflowTaskPoco(TaskType.Resubmit)
                {
                    ActionedByUserId = userId,
                    ApprovalStep = Instance.TaskInstances.Last(x => x.TaskStatus == TaskStatus.Rejected).ApprovalStep,
                    Comment = comment,
                    CompletedDate = DateTime.Now,
                    Status = (int) TaskStatus.Resubmitted,
                    WorkflowInstanceGuid = Instance.Guid,
                };

                _tasksService.InsertTask(resubmitTask);
                Instance.TaskInstances.Add(resubmitTask);

                // when approving a task for a rejected workflow, create the new task with the same approval step as the rejected task
                // update the rejected task status to resubmitted

                WorkflowTaskPoco taskInstance = Instance.CreateApprovalTask();
                SetApprovalGroup(taskInstance);
                ApproveOrContinue(taskInstance, userId);
            }
            else
            {
                if (Instance != null)
                {
                    throw new WorkflowException($"Workflow instance {Instance.Id} could not be found.");
                }
                throw new WorkflowException("Workflow instance is not found.");
            }

            // this may have been modified between workflow stages
            Instance.SetScheduledDate();
            _instancesService.UpdateInstance(Instance);

            return Instance;
        }

        /// <summary>
        /// Processes the action on the workflow instance and persists it to the database.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="action">The workflow action to be performed</param>
        /// <param name="userId">the user Id of the user who performed the action.</param>
        /// <param name="comment">Any comments the user has provided with the action.</param>
        /// <returns>the actioned workflow process instance entity</returns>
        public WorkflowInstancePoco ActionWorkflow(WorkflowInstancePoco instance, WorkflowAction action, int userId, string comment)
        {
            if (instance != null)
            {
                Instance = instance;

                if (Instance.WorkflowStatus.In(WorkflowStatus.PendingApproval, WorkflowStatus.Rejected, WorkflowStatus.Resubmitted))
                {
                    // if pending, update to approved or rejected
                    ProcessApprovalAction(action, userId, comment);

                    if (action == WorkflowAction.Approve)
                    {
                        // only progress if there are pending approval tasks, otherwise the flow is complete and the workflow should exit
                        int currentSteps = Instance.TaskInstances.Count(x => x.TaskStatus.In(TaskStatus.Approved, TaskStatus.NotRequired));
                        if (Instance.TotalSteps > currentSteps)
                        {
                            // create the next task, then check if it should be approved
                            // if it needs approval, 
                            WorkflowTaskPoco taskInstance = Instance.CreateApprovalTask();
                            SetApprovalGroup(taskInstance);
                            ApproveOrContinue(taskInstance, userId);
                        }
                        else
                        {
                            CompleteWorkflow();
                        }
                    }
                    else if (action == WorkflowAction.Reject)
                    {
                        Instance.Status = (int)WorkflowStatus.Rejected;

                        // do not complete workflow - this would publish the rejected changes.
                        // document is not rolled back, but must be resubmitted for publishing.
                    }
                }
                else
                {
                    throw new WorkflowException("Workflow instance " + Instance.Id + " is not pending any action.");
                }

                // this may have been modified between workflow stages
                Instance.SetScheduledDate();

                _instancesService.UpdateInstance(Instance);
            }
            else
            {
                if (Instance != null)
                {
                    throw new WorkflowException("Workflow instance " + Instance.Id + " is not found.");
                }
                throw new WorkflowException("Workflow instance is not found.");
            }

            return Instance;
        }

        /// <summary>
        /// Cancels the workflow instance and persists the changes to the database
        /// </summary>
        /// <param name="instance">The workflow instance id for the process to be cancelled</param>
        /// <param name="userId">The user who has cancelled the workflow instance</param>
        /// <param name="reason">The reason given for cancelling the workflow process.</param>
        /// <returns>The cancelled workflow process instance entity</returns>
        public WorkflowInstancePoco CancelWorkflow(WorkflowInstancePoco instance, int userId, string reason)
        {
            if (instance != null)
            {
                Instance = instance;
                
                Instance.Cancel();

                WorkflowTaskPoco taskInstance = Instance.TaskInstances.First();
                if (taskInstance != null)
                {
                    // Cancel the task and workflow instances
                    taskInstance.Cancel(userId, reason, Instance.CompletedDate);
                    _tasksService.UpdateTask(taskInstance);
                }

                // Send the notification
                _instancesService.UpdateInstance(Instance);
                _emailer.Send(Instance, EmailType.WorkflowCancelled);

                // emit an event
                Cancelled?.Invoke(this, new InstanceEventArgs(Instance));
            }
            else
            {
                if (Instance != null)
                {
                    throw new WorkflowException("Workflow instance " + Instance.Id + " is not found.");
                }
                throw new WorkflowException("Workflow instance is not found.");
            }

            return Instance;
        }

        protected abstract void CompleteWorkflow();

        #endregion

        #region private methods

        /// <summary>
        /// Check if the task requires approval
        /// If so, update the task and send notifications
        /// Otherwise, update the task to notrequired, resolve it and create the next step in ActionWorkflow
        /// </summary>
        private void ApproveOrContinue(WorkflowTaskPoco taskInstance, int? userId = null, string comment = "APPROVAL NOT REQUIRED")
        {
            // require approval if author is not in the approving group, or flow type is explicit, 
            // and the task has NOT been marked as not required 
            //(this would happen if the step is not required due to a conditional property not being dirty)
            if ((!taskInstance.UserGroup.IsMember(Instance.AuthorUserId) || _settings.FlowType == (int)FlowType.Explicit) &&
                taskInstance.TaskStatus != TaskStatus.NotRequired)
            {
                // set instance and task to pending, send notifications and persist task 
                taskInstance.Status = (int)TaskStatus.PendingApproval;
                Instance.Status = (int)WorkflowStatus.PendingApproval;

                _emailer.Send(Instance, EmailType.ApprovalRequest);

                _tasksService.UpdateTask(taskInstance);
            }
            else
            {
                taskInstance.Status = (int)TaskStatus.NotRequired;
                ActionWorkflow(Instance, WorkflowAction.Approve, userId ?? Instance.AuthorUserId, comment);
            }
        }

        /// <summary>
        /// Update the workflow task status to approve or reject
        /// Sets flag to send email notification if required
        /// Persists all cahanges to the task (stats, completed date, actioned by and comment)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="userId"></param>
        /// <param name="comment"></param>
        private void ProcessApprovalAction(WorkflowAction action, int userId, string comment)
        {
            WorkflowTaskPoco taskInstance = Instance.TaskInstances.First(ti => ti.CompletedDate == null);

            if (taskInstance == null) return;

            EmailType? emailAction = taskInstance.ProcessApproval(action, userId, comment);

            // Send the email after we've done the updates.
            if (emailAction != null)
            {
                _emailer.Send(Instance, emailAction.Value);
            }

            _tasksService.UpdateTask(taskInstance);
        }

        /// <summary>
        /// Find the next approval group where the current user and change author are not members
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="nodeId"></param>
        /// <param name="initialId"></param>
        private void SetApprovalGroup(WorkflowTaskPoco taskInstance, int nodeId = int.MinValue, int initialId = int.MinValue)
        {
            if (nodeId == int.MinValue)
            {
                nodeId = Instance.NodeId;
            }
            if (initialId == int.MinValue)
            {
                initialId = Instance.NodeId;
            }      

            List<UserGroupPermissionsPoco> approvalGroup = _configService.GetPermissionsForNode(nodeId);
            UserGroupPermissionsPoco group = null;

            // if the node has a approval flow set, this value will be the assigned groups
            if (approvalGroup.Any())
            {
                // approval group length will match the number of groups mapped to the node
                // only interested in the one that corresponds with the index of the most recently added workflow task
                group = approvalGroup.First(g => g.Permission == taskInstance.ApprovalStep);
                Instance.SetTotalSteps(approvalGroup.Count);
            }
            else
            {
                // check the content type only on original node - ie, not when we are recursively searching for a flow to inherit
                if (nodeId == initialId)
                {
                    List<UserGroupPermissionsPoco> contentTypeApproval = _configService.GetPermissionsForNode(0, Instance.Node.ContentType.Id);

                    if (contentTypeApproval.Any(g => g.ContentTypeId != 0))
                    {
                        // check that the current step is not excluded by a condition
                        UserGroupPermissionsPoco activeGroup = contentTypeApproval.First(g => g.Permission == taskInstance.ApprovalStep);
                        if (activeGroup != null)
                        {
                            string[] conditions = activeGroup.Condition?.Split(',');
                            // if any conditions exist for the content type,
                            // fetch the current saved version and the live version
                            // take the property value by key (the condition identifier) and compare from both docs
                            // if they match, the step is not required.
                            if (conditions != null)
                            {
                                IContent node = _utility.GetContent(nodeId);
                                IEnumerable<Property> propsToCheck =
                                    node.Properties.Where(p => conditions.Contains(p.PropertyType.Key.ToString()));

                                IPublishedContent publishedVersion = _utility.GetPublishedContent(nodeId);
                                
                                foreach (Property prop in propsToCheck)
                                {
                                    if (prop.Value.ToString().Equals(_utility.GetPropertyValueAsString(publishedVersion, prop.Alias)))
                                    {
                                        // prop is clean and matches on a condition - group should not be included
                                        taskInstance.Status = (int)TaskStatus.NotRequired;
                                    }
                                }
                            }

                            group = activeGroup;
                        }

                        Instance.SetTotalSteps(contentTypeApproval.Count);
                    }
                }

                // don't overwrite or recurse up if the content type has permissions set
                // group will still be null if no content type flow found
                if (group == null)
                {
                    // If nothing set for the content type recurse up the tree until we find something
                    IPublishedContent node = _utility.GetPublishedContent(nodeId);
                    // if we hit the homepage, don't go any higher
                    if (node.Level > 1)
                    {
                        SetApprovalGroup(taskInstance, node.Parent.Id, nodeId);
                    }
                    else // no group set, fallback to default approver
                    {
                        int groupId = int.Parse(_settings.DefaultApprover);
                        group = new UserGroupPermissionsPoco
                        {
                            GroupId = groupId,
                            UserGroup = GetGroup(groupId).Result
                        };
                        Instance.SetTotalSteps(1);
                    }
                }
            }

            _instancesService.UpdateInstance(Instance);

            // group will not be null unless we have nothing set anywhere, which is silly
            // in fact, this is likely not even possible any more
            if (group == null) return;

            taskInstance.GroupId = group.GroupId;
            taskInstance.UserGroup = group.UserGroup;

            _tasksService.InsertTask(taskInstance);
        }

        /// <summary>
        /// Helper to grab the user group when populating the default approver
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<UserGroupPoco> GetGroup(int id)
        {
            return await _groupService.GetPopulatedUserGroupAsync(id);
        }

        #endregion
    }
}
