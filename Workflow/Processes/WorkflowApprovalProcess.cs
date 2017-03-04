using log4net;
using Newtonsoft.Json;
using System;
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
            var g = Guid.NewGuid();

            instance = new WorkflowInstancePoco(nodeId, authorUserId, authorComment, Type);
            instance.Guid = g;

            bool doPublish = false;
            WorkflowTaskInstancePoco taskInstance = CreateApprovalTask(nodeId, authorUserId, out doPublish);

            if (taskInstance.UserGroup == null)
            {
                string errorMessage = "No approval flow set for document " + nodeId + " or any of its parent documents. Unable to initiate approval task.";
                Log.Error(errorMessage);
                throw new WorkflowException(errorMessage);
            }

            if (!doPublish)
            {
                if (ApprovalRequired(taskInstance))
                {
                    InitiateApprovalTask(taskInstance);
                    taskInstance.WorkflowInstanceGuid = g;
                    GetDb().Insert(taskInstance);
                    GetDb().Insert(instance);
                }
                else {
                    instance.Status = (int)WorkflowStatus.PendingApproval;
                    taskInstance.WorkflowInstanceGuid = g;
                    GetDb().Insert(taskInstance);
                    GetDb().Insert(instance);

                    ActionWorkflow(instance, WorkflowAction.Approve, authorUserId, string.Empty);
                }

            }
            else
            {
                CompleteWorkflow(authorUserId);                
            }

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
                    case (int)WorkflowStatus.PendingApproval:
                        // if pending, find the next approval group
                        ProcessApprovalAction(action, userId, comment);
                        if (action == WorkflowAction.Approve)
                        {
                            var doPublish = false;
                            WorkflowTaskInstancePoco taskInstance = CreateApprovalTask(instance.NodeId, userId, out doPublish);
                            if (ApprovalRequired(taskInstance))
                            {
                                InitiateApprovalTask(taskInstance);
                                taskInstance.WorkflowInstanceGuid = instance.Guid;
                                GetDb().Insert(taskInstance);
                            }
                            else
                            {
                                CompleteWorkflow(userId);
                            }
                        }
                        break;
                    default:
                        throw new WorkflowException("Workflow instance " + instance.Id + " is not pending any action.");
                }
                GetDb().Update(instance);
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

        public abstract void CompleteWorkflow(int userId);

        #endregion

        #region private methods
        private void ProcessApprovalAction(WorkflowAction action, int userId, string comment)
        {
            var taskInstance = instance.TaskInstances.FirstOrDefault(ti => ti._Status == TaskStatus.PendingApproval || ti._Status == TaskStatus.New );

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
            taskInstance.Comment = comment;
            taskInstance.ActionedByUserId = userId;

            // Send the email after we've done the updates.
            if (emailRequired)
            {
                Notifications.Send(instance, emailType.Value);
            }

            GetDb().Update(taskInstance);
        }

        /// <summary>
        /// TODO : find next group in approval path where current author is not a member - shouldn't need to approve own work
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="nodeId"></param>
        /// <param name="authorId"></param>
        private bool SetApprovalGroup(WorkflowTaskInstancePoco taskInstance, int nodeId, int authorId, int groupIndex)
        {
            // 0 is the first group set on a given node workflow path
            var contentTypeId = Helpers.GetNode(nodeId).ContentType.Id;
            var approvalGroup = _pr.PermissionsForNode(nodeId, contentTypeId);

            if (approvalGroup.Any())
            {
                // approval group length will match the number of groups mapped to the node
                // only interested in the one that corresponds with the index of the most recently added group
                // also check the flow type = defined or contenttype, then use the appropriate group reference
                var byContentType = approvalGroup.Where(g => g.ContentTypeId != 0);
                UserGroupPermissionsPoco group;
                if (byContentType.Any())
                {
                    group = byContentType.Where(g => g.Permission == groupIndex).First();
                }
                else {
                    group = approvalGroup.Where(g => g.Permission == groupIndex).First();
                }
                // This node has a permission set directly so use its group.
                taskInstance.GroupId = group.GroupId;
                taskInstance.UserGroup = group.UserGroup;

                // check the last permission is equal to the current group, if so, the request should be published
                return (byContentType.Any() ? byContentType : approvalGroup).OrderBy(g => g.Permission).Last().Permission == groupIndex;
            }
            else
            {
                // Recurse up the tree until we find something
                var node = Helpers.GetNode(nodeId);
                if (node.Level != 1)
                {
                    SetApprovalGroup(taskInstance, node.Parent.Id, authorId, groupIndex);
                }
                else // no coordinator set, default to final approver group
                {
                    var settings = _pr.GetSettings();
                    var finalApproverGroup = GetDb().Fetch<UserGroupPermissionsPoco>(SqlHelpers.UserGroupBasic, settings.DefaultApprover).First();

                    taskInstance.GroupId = finalApproverGroup.GroupId;
                    taskInstance.UserGroup = finalApproverGroup.UserGroup;
                }

                return false;
            }
        }

        private WorkflowTaskInstancePoco CreateApprovalTask(int nodeId, int authorId, out bool doPublish)
        {
            // creating initial task should use perm = 0
            // subsequent tasks increment this index
            var taskInstance = new WorkflowTaskInstancePoco(TaskType.Approve);
            taskInstance.ApprovalStep = instance.TaskInstances.Count;
            instance.TaskInstances.Add(taskInstance);

            doPublish = SetApprovalGroup(taskInstance, nodeId, authorId, instance.TaskInstances.Count - 1);

            //taskInstance.UserGroup = GetDb().Fetch<UserGroupPoco, User2UserGroupPoco, UserGroupPoco>(
            //    new UserToGroupRelator().MapIt,
            //    SqlHelpers.UserGroupWithUsersById,
            //    taskInstance.GroupId).First();          

            return taskInstance;
        }

        /// <summary>
        /// Determines whether coordinator approval is required by checking if the Author is in the coordinator group for the page.
        /// </summary>
        /// <returns>true if approval required, false otherwise</returns>
        private bool ApprovalRequired(WorkflowTaskInstancePoco taskInstance)
        {
            // check here will be for the number of groups with permission on the current node - if 1, return false as the workflow is single stage

            // Approval required if the author is not in the current group.
            if (!taskInstance.UserGroup.IsMember(instance.AuthorUserId))
            {
                return true;
            }
            else
            {    // Not required
                taskInstance.Status = (int)TaskStatus.PendingApproval;
                return false;
            }
        }

        private void InitiateApprovalTask(WorkflowTaskInstancePoco taskInstance)
        {
            // Set task and workflow information.
            taskInstance.Status = (int)TaskStatus.PendingApproval;
            instance.Status = (int)WorkflowStatus.PendingApproval;

            Notifications.Send(instance, EmailType.ApprovalRequest);
        }

        private WorkflowTaskInstancePoco CreateFinalApprovalTask()
        {
            var taskInstance = new WorkflowTaskInstancePoco(TaskType.Publish);
            instance.TaskInstances.Add(taskInstance);
            string finalApprover = Helpers.GetSettings().DefaultApprover;
            if (!string.IsNullOrEmpty(finalApprover))
            {
                taskInstance.UserGroup = GetDb().Fetch<UserGroupPoco, UserGroupPermissionsPoco, User2UserGroupPoco, UserGroupPoco>(
                    new GroupsRelator().MapIt,
                    SqlHelpers.UserGroupDetailed,
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
            instance.Status = (int)WorkflowStatus.PendingApproval;

            Notifications.Send(instance, EmailType.ApprovalRequest);
        }

        # endregion
    }
}
