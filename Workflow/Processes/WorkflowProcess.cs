using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Workflow.Events.Args;
using Workflow.Extensions;
using Workflow.Helpers;
using Workflow.Models;
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
        private readonly ISettingsService _settingsService;
        private readonly ITasksService _tasksService;

        private readonly Notifications _notifications;

        private readonly WorkflowSettingsPoco _settings;

        protected WorkflowType Type { private get; set; }
        protected WorkflowInstancePoco Instance;

        public static event EventHandler<InstanceEventArgs> Created;
        public static event EventHandler<InstanceEventArgs> Cancelled;

        protected WorkflowProcess()
        {
            _configService = new ConfigService();
            _groupService = new GroupService();
            _instancesService = new InstancesService();
            _settingsService = new SettingsService();
            _tasksService = new TasksService();

            _notifications = new Notifications();
            
            _settings = _settingsService.GetSettings();
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
            // use the guid to associate tasks to a workflow instance
            Guid g = Guid.NewGuid();

            // create and persist the new workflow instance
            Instance = new WorkflowInstancePoco(nodeId, authorUserId, authorComment, Type);
            Instance.SetScheduledDate();
            Instance.Guid = g;

            _instancesService.InsertInstance(Instance);

            // create the first task in the workflow
            WorkflowTaskInstancePoco taskInstance = CreateApprovalTask(nodeId);
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
                var resubmitTask = new WorkflowTaskInstancePoco(TaskType.Resubmit)
                {
                    ActionedByUserId = userId,
                    ApprovalStep = Instance.TaskInstances.Last(x => x.TaskStatus == TaskStatus.Rejected).ApprovalStep,
                    Comment = comment,
                    CompletedDate = DateTime.Now,
                    Status = (int) TaskStatus.Resubmitted,
                    WorkflowInstanceGuid = Instance.Guid
                };

                _tasksService.InsertTask(resubmitTask);

                // when approving a task for a rejected workflow, create the new task with the same approval step as the rejected task
                // update the rejected task status to resubmitted

                WorkflowTaskInstancePoco taskInstance = CreateApprovalTask(Instance.NodeId);
                ApproveOrContinue(taskInstance, userId);

                WorkflowTaskInstancePoco rejectedTask = Instance.TaskInstances.Last(x => x.TaskStatus == TaskStatus.Rejected);
                rejectedTask.Status = (int) TaskStatus.Resubmitted;
                rejectedTask.Type = (int) TaskType.Rejected;

                _tasksService.UpdateTask(rejectedTask);
            }
            else
            {
                if (Instance != null)
                {
                    throw new WorkflowException("Workflow instance " + Instance.Id + " is not found.");
                }
                throw new WorkflowException("Workflow instance is not found.");
            }

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

                if (Instance.WorkflowStatus == WorkflowStatus.PendingApproval)
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
                            WorkflowTaskInstancePoco taskInstance = CreateApprovalTask(Instance.NodeId);
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
                Instance.CompletedDate = DateTime.Now;
                Instance.Status = (int)WorkflowStatus.Cancelled;

                WorkflowTaskInstancePoco taskInstance = Instance.TaskInstances.Last();
                if (taskInstance != null)
                {
                    // Cancel the task and workflow instances
                    taskInstance.Status = (int)TaskStatus.Cancelled;
                    taskInstance.ActionedByUserId = userId;
                    taskInstance.Comment = reason;
                    taskInstance.CompletedDate = Instance.CompletedDate;

                    _tasksService.UpdateTask(taskInstance);
                }

                // Send the notification
                _instancesService.UpdateInstance(Instance);
                _notifications.Send(Instance, EmailType.WorkflowCancelled);

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
        private void ApproveOrContinue(WorkflowTaskInstancePoco taskInstance, int? userId = null, string comment = "APPROVAL NOT REQUIRED")
        {
            // if author is not in the approving group, or flow type is explicit, require approval
            if (!taskInstance.UserGroup.IsMember(Instance.AuthorUserId) || _settings.FlowType == (int)FlowType.Explicit)
            {
                SetPendingTask(taskInstance);
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
            // tasks are ordered by approval step, so could probably take the last, but best to also check for the correct status.
            WorkflowTaskInstancePoco taskInstance = Instance.TaskInstances.LastOrDefault(x => x.TaskStatus != TaskStatus.Approved);

            if (taskInstance == null) return;

            EmailType? emailType = null;
            var emailRequired = false;

            switch (action)
            {
                case WorkflowAction.Approve:
                    if (taskInstance.TaskStatus != TaskStatus.NotRequired)
                    {
                        taskInstance.Status = (int) TaskStatus.Approved;
                    }
                    break;

                case WorkflowAction.Reject:
                    taskInstance.Status = (int)TaskStatus.Rejected;
                    emailRequired = true;
                    emailType = EmailType.ApprovalRejection;

                    break;
            }

            taskInstance.CompletedDate = DateTime.Now;
            taskInstance.Comment = comment;
            taskInstance.ActionedByUserId = userId;

            // Send the email after we've done the updates.
            if (emailRequired)
            {
                _notifications.Send(Instance, emailType.Value);
            }

            _tasksService.UpdateTask(taskInstance);
        }

        /// <summary>
        /// Generate the next approval flow task, returning the new task and a bool indicating whether the publish action should becompleted (ie, this is the end of the flow)
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private WorkflowTaskInstancePoco CreateApprovalTask(int nodeId)
        {
            var taskInstance =
                new WorkflowTaskInstancePoco(TaskType.Approve)
                {
                    ApprovalStep = Instance.TaskInstances.Count(x => x.TaskStatus.In(TaskStatus.Approved, TaskStatus.NotRequired)),
                    WorkflowInstanceGuid = Instance.Guid
                };

            Instance.TaskInstances.Add(taskInstance);

            SetApprovalGroup(taskInstance, nodeId, nodeId);

            _tasksService.InsertTask(taskInstance);

            return taskInstance;
        }

        /// <summary>
        /// Find the next approval group where the current user and change author are not members
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="nodeId"></param>
        /// <param name="initialId"></param>
        private void SetApprovalGroup(WorkflowTaskInstancePoco taskInstance, int nodeId, int initialId)
        {
            List<UserGroupPermissionsPoco> approvalGroup = _configService.GetPermissionsForNode(nodeId, 0);
            UserGroupPermissionsPoco group = null;

            // if the node has a approval flow set, this value will be the assigned groups
            if (approvalGroup.Any())
            {
                // approval group length will match the number of groups mapped to the node
                // only interested in the one that corresponds with the index of the most recently added workflow task
                group = approvalGroup.First(g => g.Permission == taskInstance.ApprovalStep);
                SetInstanceTotalSteps(approvalGroup.Count);
            }
            else
            {
                // check the content type only on original node - ie, not when we are recursively searching for a flow to inherit
                if (nodeId == initialId)
                {
                    List<UserGroupPermissionsPoco> contentTypeApproval = _configService.GetPermissionsForNode(0, Instance.Node.ContentType.Id);

                    if (contentTypeApproval.Any(g => g.ContentTypeId != 0))
                    {
                        group = contentTypeApproval.First(g => g.Permission == taskInstance.ApprovalStep);
                        SetInstanceTotalSteps(contentTypeApproval.Count);
                    }
                }

                // don't overwrite or recurse up if the content type has permissions set
                // group will still be null if no content type flow found
                if (group == null)
                {
                    // If nothing set for the content type recurse up the tree until we find something
                    IPublishedContent node = Utility.GetNode(nodeId);
                    if (node.Level != 1)
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
                        SetInstanceTotalSteps(1);
                    }
                }
            }

            // group will not be null unless we have nothing set anywhere, which is silly
            if (group == null) return;

            taskInstance.GroupId = group.GroupId;
            taskInstance.UserGroup = group.UserGroup;
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

        /// <summary>
        /// set the total steps property for a workflow instance
        /// </summary>
        /// <param name="stepCount">The number of approval groups in the current flow (explicit, inherited or content type)</param>
        private void SetInstanceTotalSteps(int stepCount)
        {
            if (Instance.TotalSteps == stepCount) return;

            Instance.TotalSteps = stepCount;
            _instancesService.UpdateInstance(Instance);
        }


        /// <summary>
        /// Set the task to pending, notify appropriate groups
        /// </summary>
        /// <param name="taskInstance"></param>
        private void SetPendingTask(WorkflowTaskInstancePoco taskInstance)
        {
            taskInstance.Status = (int)TaskStatus.PendingApproval;
            Instance.Status = (int)WorkflowStatus.PendingApproval;

            _notifications.Send(Instance, EmailType.ApprovalRequest);

            _tasksService.UpdateTask(taskInstance);
        }

        #endregion
    }
}
