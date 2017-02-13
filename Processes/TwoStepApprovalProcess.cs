using log4net;
using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Persistence;
using Workflow.Models;
using Workflow.Relators;

namespace Workflow
{
    public abstract class TwoStepApprovalProcess : IWorkflowProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected Database db;
        protected WorkflowType Type { get; set; }
        protected WorkflowInstancePoco instance;

        public TwoStepApprovalProcess(Database db)
        {
            this.db = db;
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
            var g = Guid.NewGuid();

            instance = new WorkflowInstancePoco(nodeId, authorUserId, authorComment, Type);
            instance.Guid = g;

            WorkflowTaskInstancePoco coordinatorTaskInstance = CreateCoordinatorApprovalTask(nodeId);

            if (coordinatorTaskInstance.UserGroup == null)
            {
                string errorMessage = "No workflow coordinator user group set for document " + nodeId + " or any of its parent documents. Unable to initiate coordinator approval task.";
                Log.Error(errorMessage);
                throw new WorkflowException(errorMessage);
            }

            if (IsCoordinatorApprovalRequired(coordinatorTaskInstance))
            {
                InitiateCoordinatorApprovalTask(coordinatorTaskInstance);
                coordinatorTaskInstance.WorkflowInstanceGuid = g;
                db.Insert(coordinatorTaskInstance);
            }
            else
            {
                WorkflowTaskInstancePoco finalTaskInstance = CreateFinalApprovalTask();
                if (IsFinalApprovalRequired(finalTaskInstance))
                {
                    InitiateFinalApprovalTask(finalTaskInstance);
                }
                else
                {
                    CompleteWorkflow(authorUserId);
                }
            }

            db.Insert(instance);            

            return instance;
        }

        /// <summary>
        /// Processes the action on the workflow instance with the given Id and persists it to the database.
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

                switch (instance.Status)
                {
                    case (int)WorkflowStatus.PendingCoordinatorApproval:
                        ProcessApprovalAction(action, userId, comment);
                        if (action == WorkflowAction.Approve)
                        {
                            WorkflowTaskInstancePoco finalTaskInstance = CreateFinalApprovalTask();
                            if (IsFinalApprovalRequired(finalTaskInstance))
                            {
                                InitiateFinalApprovalTask(finalTaskInstance);
                                finalTaskInstance.WorkflowInstanceGuid = instance.Guid;
                                db.Insert(finalTaskInstance);
                            }
                            else
                            {
                                CompleteWorkflow(userId);
                            }
                        }
                        break;
                    case (int)WorkflowStatus.PendingFinalApproval:
                        ProcessApprovalAction(action, userId, comment);
                        if (action == WorkflowAction.Approve)
                        {
                            CompleteWorkflow(userId);
                        }
                        break;
                    default:
                        throw new WorkflowException("Workflow instance " + instance.Id + " is not pending any action.");
                }
                db.Update(instance);
            }
            else
            {
                if (instance != null)
                    throw new WorkflowException("Workflow instance " + instance.Id + " is not found.");
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

                var taskInstance = instance.TaskInstances.FirstOrDefault(ti => ti._Status == Workflow.Models.TaskStatus.PendingApproval);
                if (taskInstance != null)
                {
                    // Cancel the task and workflow instances
                    taskInstance.Status = (int)Workflow.Models.TaskStatus.Cancelled;
                    taskInstance.ActionedByUserId = userId;
                    taskInstance.Comment = reason;
                    taskInstance.CompletedDate = instance.CompletedDate;

                    db.Update(taskInstance);
                }

                // Send the notification
                db.Update(instance);
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

        public abstract void CompleteWorkflow(int userId);

        #endregion

        #region private methods
        private void ProcessApprovalAction(WorkflowAction action, int userId, string comment)
        {
            var taskInstance = instance.TaskInstances.FirstOrDefault(ti => ti._Status == Workflow.Models.TaskStatus.PendingApproval);

            EmailType? emailType = null;
            bool emailRequired = false;

            switch (action)
            {
                case WorkflowAction.Approve:
                    taskInstance.Status = (int)Workflow.Models.TaskStatus.Approved;
                    break;

                case WorkflowAction.Reject:
                    instance.Status = (int)WorkflowStatus.Rejected;
                    instance.CompletedDate = DateTime.Now;
                    taskInstance.Status = (int)Workflow.Models.TaskStatus.Rejected;
                    emailRequired = true;
                    if (taskInstance._Type == TaskType.CoordinatorApproval)
                    {
                        emailType = EmailType.CoordinatorApprovalRejection;
                    }
                    else
                    {
                        emailType = EmailType.FinalApprovalRejection;
                    }
                    break;
            }

            taskInstance.CompletedDate = DateTime.Now;
            taskInstance.Comment = comment;
            taskInstance.ActionedByUserId = userId;

            // Send the email after we've done the updates.
            if (emailRequired)
            {
                Notifications.Send(instance, emailType.Value);
            }

            db.Update(taskInstance);
        }

        private void GetCoordinatorUserGroup(WorkflowTaskInstancePoco taskInstance, int nodeId)
        {
            var wfCoordNodePerm = db.Fetch<UserGroupPermissionsPoco>(SqlHelpers.PermissionsByNodeAndType, nodeId, 2);

            if (wfCoordNodePerm.Any())
            {
                // This node has a permission set directly so use its group.
                taskInstance.GroupId = wfCoordNodePerm.First().GroupId;
                taskInstance.UserGroup = wfCoordNodePerm.First().UserGroup;
            }
            else
            {
                // Recurse up the tree until we find something
                var node = Helpers.GetNode(nodeId);
                if (node.Level != 1)
                {
                    GetCoordinatorUserGroup(taskInstance, node.Parent.Id);
                }
            }
        }

        private WorkflowTaskInstancePoco CreateCoordinatorApprovalTask(int nodeId)
        {
            var taskInstance = new WorkflowTaskInstancePoco(TaskType.CoordinatorApproval);
            instance.TaskInstances.Add(taskInstance);
            GetCoordinatorUserGroup(taskInstance, nodeId);
            taskInstance.UserGroup = db.Fetch<UserGroupPoco, User2UserGroupPoco, UserGroupPoco>(
                new UserToGroupRelator().MapIt, 
                SqlHelpers.UserGroupWithUsersById,
                taskInstance.GroupId).First();
            return taskInstance;
        }

        /// <summary>
        /// Determines whether coordinator approval is required by checking if the Author is in the coordinator group for the page.
        /// </summary>
        /// <returns>true if approval required, false otherwise</returns>
        private bool IsCoordinatorApprovalRequired(WorkflowTaskInstancePoco taskInstance)
        {
            // Approval required if the author is not in the coordinator group.
            if (!taskInstance.UserGroup.IsMember(instance.AuthorUserId))
            {
                return true;
            }
            else
            {    // Not required
                taskInstance.Status = (int)TaskStatus.NotRequired;
                return false;
            }
        }

        private void InitiateCoordinatorApprovalTask(WorkflowTaskInstancePoco taskInstance)
        {
            // Set task and workflow information.
            taskInstance.Status = (int)TaskStatus.PendingApproval;
            instance.Status = (int)WorkflowStatus.PendingCoordinatorApproval;

            Notifications.Send(instance, EmailType.CoordinatorApprovalRequest);
        }

        private WorkflowTaskInstancePoco CreateFinalApprovalTask()
        {
            var taskInstance = new WorkflowTaskInstancePoco(TaskType.FinalApproval);
            instance.TaskInstances.Add(taskInstance);
            string finalApprover = Helpers.GetSettings().FinalApprover;
            if (!string.IsNullOrEmpty(finalApprover))
            {
                taskInstance.UserGroup = db.Fetch<UserGroupPoco, User2UserGroupPoco, UserGroupPoco>(
                    new UserToGroupRelator().MapIt,
                    SqlHelpers.UserGroupWithUsersById,
                    finalApprover).First();
            }
            if (taskInstance.UserGroup != null)
            {
                taskInstance.GroupId = taskInstance.UserGroup.GroupId;
            }
            else
            {
                throw new WorkflowException("Unable to determine final approval user group. Site settings need to be configured.");
            }

            return taskInstance;
        }

        /// <summary>
        /// Determines whether final approval is required by checking if the Author is in the if the Author is in the Final approvers group.
        /// </summary>
        /// <returns>true if final approval required, false otherwise</returns>
        private bool IsFinalApprovalRequired(WorkflowTaskInstancePoco taskInstance)
        {
            // Approval required if the author is not in the final approvers group.
            if (!taskInstance.UserGroup.IsMember(instance.AuthorUserId) && Helpers.IsNotFastTrack(instance))
            {
                return true;
            }
            // Not required
            taskInstance.Status = (int)TaskStatus.NotRequired;
            return false;
        }

        private void InitiateFinalApprovalTask(WorkflowTaskInstancePoco taskInstance)
        {
            // Set task and workflow information.
            taskInstance.Status = (int)TaskStatus.PendingApproval;
            instance.Status = (int)WorkflowStatus.PendingFinalApproval;

            Notifications.Send(instance, EmailType.FinalApprovalRequest);
        }

        # endregion
    }
}
