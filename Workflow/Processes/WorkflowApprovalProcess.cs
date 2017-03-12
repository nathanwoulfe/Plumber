using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Workflow.Models;
using Workflow.Relators;

namespace Workflow
{
    public abstract class WorkflowApprovalProcess : IWorkflowProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static PocoRepository _pr = new PocoRepository();

        protected WorkflowType Type { get; set; }
        protected WorkflowInstancePoco instance;

        private Database GetDb()
        {
            return ApplicationContext.Current.DatabaseContext.Database;
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
            var g = Guid.NewGuid();

            // create and persist the new workflow instance
            instance = new WorkflowInstancePoco(nodeId, authorUserId, authorComment, Type);
            instance.SetScheduledDate();
            instance.Guid = g;

            GetDb().Insert(instance);

            // create the first task in the workflow
            bool complete = false;
            WorkflowTaskInstancePoco taskInstance = CreateApprovalTask(nodeId, authorUserId, out complete);

            if (taskInstance.UserGroup == null)
            {
                string errorMessage = "No approval flow set for document " + nodeId + " or any of its parent documents. Unable to initiate approval task.";
                Log.Error(errorMessage);
                throw new WorkflowException(errorMessage);
            }

            ApproveOrContinue(taskInstance, authorUserId);

            return instance;
        }

        /// <summary>
        /// Processes the action on the workflow instance and persists it to the database.
        /// </summary>
        /// <param name="instanceId">The worflow instance id to process the action on</param>
        /// <param name="action">The workflow action to be performed</param>
        /// <param name="userId">the user Id of the user who performed the action.</param>
        /// <param name="comment">Any comments the user has provided with the action.</param>
        /// <returns>the actioned workflow process instance entity</returns>
        public WorkflowInstancePoco ActionWorkflow(WorkflowInstancePoco _instance, WorkflowAction action, int userId, string comment)
        {
            if (_instance != null)
            {
                instance = _instance;

                if (instance.Status == (int)WorkflowStatus.PendingApproval) 
                {
                    // if pending, update to approved
                    ProcessApprovalAction(action, userId, comment);
                    if (action == WorkflowAction.Approve)
                    {
                        // only progress if there are pending approval tasks, otherwise the flow is complete and the workflow should exit
                        if (instance.TotalSteps > instance.TaskInstances.Count)
                        {
                            // create the next task, then check if it should be a
                            var approvalRequired = false;
                            WorkflowTaskInstancePoco taskInstance = CreateApprovalTask(instance.NodeId, userId, out approvalRequired);
                            if (approvalRequired)
                            {
                                ApproveOrContinue(taskInstance, userId);
                            }
                            else
                            {
                                CompleteTask(taskInstance, userId);
                                CompleteWorkflow();
                            }
                        }
                        else
                        {
                            CompleteWorkflow();
                        }
                    }
                }
                else
                {
                    throw new WorkflowException("Workflow instance " + instance.Id + " is not pending any action.");
                }
                GetDb().Update(instance);
            }
            else
            {
                if (instance != null)
                {
                    throw new WorkflowException("Workflow instance " + instance.Id + " is not found.");
                }
                throw new WorkflowException("Workflow instance is not found.");
            }
            return instance;
        }

        /// <summary>
        /// Cancels the workflow instance and persists the changes to the database
        /// </summary>
        /// <param name="instanceId">The workflow instance id for the process to be cancelled</param>
        /// <param name="userId">The user who has cancelled the workflow instance</param>
        /// <param name="reason">The reason given for cancelling the workflow process.</param>
        /// <returns>The cancelled workflow process instance entity</returns>
        public WorkflowInstancePoco CancelWorkflow(WorkflowInstancePoco _instance, int userId, string reason)
        {
            if (_instance != null)
            {
                instance = _instance;
                instance.CompletedDate = DateTime.Now;
                instance.Status = (int)WorkflowStatus.Cancelled;

                var taskInstance = instance.TaskInstances.FirstOrDefault(ti => ti._Status == TaskStatus.PendingApproval);
                if (taskInstance != null)
                {
                    // Cancel the task and workflow instances
                    taskInstance.Status = (int)TaskStatus.Cancelled;
                    taskInstance.ActionedByUserId = userId;
                    taskInstance.Comment = reason;
                    taskInstance.CompletedDate = instance.CompletedDate;

                    GetDb().Update(taskInstance);
                }

                // Send the notification
                GetDb().Update(instance);
                Notifications.Send(instance, EmailType.WorkflowCancelled);
            }
            else
            {
                if (instance != null)
                {
                    throw new WorkflowException("Workflow instance " + instance.Id + " is not found.");
                }
                throw new WorkflowException("Workflow instance is not found.");
            }

            return instance;
        }

        public abstract void CompleteWorkflow();

        #endregion

        #region private methods
        /// <summary>
        /// Process task - approval is not required if current user is a member of the approving group
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="userId"></param>
        /// <param name="g"></param>
        private void ApproveOrContinue(WorkflowTaskInstancePoco taskInstance, int userId)
        {
            if (IsStepApprovalRequired(taskInstance))
            {
                InitiateApprovalTask(taskInstance);
                GetDb().Update(taskInstance);
            }
            else
            {
                instance.Status = (int)WorkflowStatus.PendingApproval;
                taskInstance.Status = (int)TaskStatus.NotRequired;
                taskInstance.Comment = taskInstance.Comment + " (APPROVAL AT STAGE " + (taskInstance.ApprovalStep + 1) + " NOT REQUIRED)";
                GetDb().Update(taskInstance);
                GetDb().Update(instance);

                ActionWorkflow(instance, WorkflowAction.Approve, userId, string.Empty);
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
            var taskInstance = instance.TaskInstances.FirstOrDefault(ti => ti._Status == TaskStatus.PendingApproval || ti._Status == TaskStatus.NotRequired );

            EmailType? emailType = null;
            bool emailRequired = false;

            switch (action)
            {
                case WorkflowAction.Approve:
                    taskInstance.Status = (int)TaskStatus.Approved;
                    break;

                case WorkflowAction.Reject:
                    instance.Status = (int)WorkflowStatus.Rejected;
                    instance.CompletedDate = DateTime.Now;
                    taskInstance.Status = (int)TaskStatus.Rejected;
                    emailRequired = true;
                    emailType = EmailType.ApprovalRejection;

                    break;
            }

            taskInstance.CompletedDate = DateTime.Now;
            taskInstance.Comment = !string.IsNullOrEmpty(comment) ? comment : taskInstance.Comment;
            taskInstance.ActionedByUserId = userId;

            // Send the email after we've done the updates.
            if (emailRequired)
            {
                Notifications.Send(instance, emailType.Value);
            }

            GetDb().Update(taskInstance);
        }

        /// <summary>
        /// Generate the next approval flow task, returning the new task and a bool indicating whether the publish action should becompleted (ie, this is the end of the flow)
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="authorId"></param>
        /// <param name="doPublish"></param>
        /// <returns></returns>
        private WorkflowTaskInstancePoco CreateApprovalTask(int nodeId, int authorId, out bool approvalRequired)
        {
            var taskInstance = new WorkflowTaskInstancePoco(TaskType.Approve);
            taskInstance.ApprovalStep = instance.TaskInstances.Count;
            taskInstance.WorkflowInstanceGuid = instance.Guid;
            taskInstance.Comment = instance.AuthorComment;
            instance.TaskInstances.Add(taskInstance);

            SetApprovalGroup(taskInstance, nodeId, authorId);
            approvalRequired = IsStepApprovalRequired(taskInstance);

            GetDb().Insert(taskInstance);            

            return taskInstance;
        }

        /// <summary>
        /// Find the next approval group where the current user and change author are not members
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="nodeId"></param>
        /// <param name="authorId"></param>
        private void SetApprovalGroup(WorkflowTaskInstancePoco taskInstance, int nodeId, int authorId)
        {
            var approvalGroup = _pr.PermissionsForNode(nodeId, 0);
            UserGroupPermissionsPoco group = null;

            if (approvalGroup.Any())
            {
                // approval group length will match the number of groups mapped to the node
                // only interested in the one that corresponds with the index of the most recently added workflow task
                group = approvalGroup.Where(g => g.Permission == taskInstance.ApprovalStep).First();
                SetInstanceTotalSteps(approvalGroup.Count);
            }
            else
            {
                // Recurse up the tree until we find something
                var node = Helpers.GetNode(nodeId);
                if (node.Level != 1)
                {
                    SetApprovalGroup(taskInstance, node.Parent.Id, authorId);
                }
                else // no group set, check for content-type approval then fallback to default approver
                {
                    var contentTypeApproval = _pr.PermissionsForNode(nodeId, instance.Node.ContentType.Id).Where(g => g.ContentTypeId != 0).ToList();
                    if (contentTypeApproval.Any())
                    {
                        group = approvalGroup.Where(g => g.Permission == taskInstance.ApprovalStep).First();
                        SetInstanceTotalSteps(approvalGroup.Count);
                    }
                    else
                    {
                        group = GetDb().Fetch<UserGroupPermissionsPoco>(SqlHelpers.UserGroupBasic, _pr.GetSettings().DefaultApprover).First();
                        SetInstanceTotalSteps(1);
                    }
                }
            }

            // group will not be null
            if (group != null)
            {
                taskInstance.GroupId = group.GroupId;
                taskInstance.UserGroup = group.UserGroup;
            }
        }

        /// <summary>
        /// Is the next step required? Authors shouldn't approve their own work, nor should a user be in subsequent steps
        /// Function checks these conditions and returns a bool to indicate whether the flow should auto-advance
        /// </summary>
        /// <param name="approvalGroup"></param>
        /// <param name="group"></param>
        /// <param name="currentUserId"></param>
        /// <param name="groupIndex"></param>
        /// <returns></returns>
        private bool CheckSubsequentStep(List<UserGroupPermissionsPoco> approvalGroup, UserGroupPermissionsPoco group, int currentUserId, int groupIndex)
        {
            bool doPublish = false;
            // check the last permission is equal to the current group, and current user or instance author are members of that group, if so, the request should be published
            if (approvalGroup.OrderBy(g => g.Permission).Last().Permission == groupIndex && (group.UserGroup.IsMember(currentUserId) || group.UserGroup.IsMember(instance.AuthorUserId)))
            {
                doPublish = true;
            }
            else
            {
                // if not the last group, check that the current user or instance author aren't a member of all the subsequent groups
                // if we find a group where the current user or instance author aren't a member, the approval flow continues
                // if they are part of all subsequent groups, the workflow should advance automatically
                // this does mean the current user could be in group 2 and 4, but not 3, so they will recieve a publish request for stage 4
                foreach (var g in approvalGroup.Where(g => g.Permission >= groupIndex))
                {
                    if (g.UserGroup.IsMember(instance.AuthorUserId) || g.UserGroup.IsMember(currentUserId))
                    {
                        doPublish = true;
                    }
                    else
                    {
                        doPublish = false;
                        break;
                    }
                }
            }
            return doPublish;
        }

        /// <summary>
        /// Determines whether approval is required by checking if the Author is in the current task group.
        /// </summary>
        /// <returns>true if approval required, false otherwise</returns>
        private bool IsStepApprovalRequired(WorkflowTaskInstancePoco taskInstance)
        {
            return Helpers.GetSettings().FlowType == (int)FlowType.All || (!taskInstance.UserGroup.IsMember(instance.AuthorUserId) && !taskInstance.UserGroup.IsMember(Helpers.GetCurrentUser().Id));
        }

        /// <summary>
        /// set the total steps property for a workflow instance
        /// </summary>
        /// <param name="stepCount">The number of approval groups in the current flow (explicit, inherited or content type)</param>
        private void SetInstanceTotalSteps(int stepCount)
        {
            if (instance.TotalSteps != stepCount)
            {
                instance.TotalSteps = stepCount;
                GetDb().Update(instance);
            }
        }

        private void InitiateApprovalTask(WorkflowTaskInstancePoco taskInstance)
        {
            // Set task and workflow information.
            taskInstance.Status = (int)TaskStatus.PendingApproval;
            instance.Status = (int)WorkflowStatus.PendingApproval;

            Notifications.Send(instance, EmailType.ApprovalRequest);
        }

        /// <summary>
        /// Terminates a task, setting it to approved and updating the comment to indicate automatic approval
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="userId"></param>
        private void CompleteTask(WorkflowTaskInstancePoco taskInstance, int userId)
        {            
            taskInstance.Status = (int)TaskStatus.Approved;
            taskInstance.CompletedDate = DateTime.Now;
            taskInstance.Comment = taskInstance.Comment + " (APPROVAL AT STAGE " + (taskInstance.ApprovalStep + 1) + " NOT REQUIRED)";
            taskInstance.ActionedByUserId = userId;

            GetDb().Update(taskInstance);
        }

        #endregion
    }
}
