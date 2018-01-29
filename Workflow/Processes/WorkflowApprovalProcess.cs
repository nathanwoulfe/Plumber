using log4net;
using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Helpers;
using Workflow.Models;

namespace Workflow.Processes
{
    public abstract class WorkflowApprovalProcess : IWorkflowProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly PocoRepository Pr = new PocoRepository();

        protected WorkflowType Type { get; set; }
        protected WorkflowInstancePoco Instance;

        private static Database GetDb()
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
            Instance = new WorkflowInstancePoco(nodeId, authorUserId, authorComment, Type);
            Instance.SetScheduledDate();
            Instance.Guid = g;

            GetDb().Insert(Instance);

            // create the first task in the workflow
            var taskInstance = CreateApprovalTask(nodeId, out bool complete);

            if (taskInstance.UserGroup == null)
            {
                var errorMessage = "No approval flow set for document " + nodeId + " or any of its parent documents. Unable to initiate approval task.";
                Log.Error(errorMessage);
                throw new WorkflowException(errorMessage);
            }

            ApproveOrContinue(taskInstance, authorUserId);

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

                if (Instance.Status == (int)WorkflowStatus.PendingApproval) 
                {
                    // if pending, update to approved
                    ProcessApprovalAction(action, userId, comment);
                    if (action == WorkflowAction.Approve)
                    {
                        // only progress if there are pending approval tasks, otherwise the flow is complete and the workflow should exit
                        if (Instance.TotalSteps > Instance.TaskInstances.Count)
                        {
                            // create the next task, then check if it should be a
                            var taskInstance = CreateApprovalTask(Instance.NodeId, out bool approvalRequired);
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
                    throw new WorkflowException("Workflow instance " + Instance.Id + " is not pending any action.");
                }
                GetDb().Update(Instance);
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

                var taskInstance = Instance.TaskInstances.FirstOrDefault(ti => ti.TaskStatus == TaskStatus.PendingApproval);
                if (taskInstance != null)
                {
                    // Cancel the task and workflow instances
                    taskInstance.Status = (int)TaskStatus.Cancelled;
                    taskInstance.ActionedByUserId = userId;
                    taskInstance.Comment = reason;
                    taskInstance.CompletedDate = Instance.CompletedDate;

                    GetDb().Update(taskInstance);
                }

                // Send the notification
                GetDb().Update(Instance);
                Notifications.Send(Instance, EmailType.WorkflowCancelled);
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

        public abstract void CompleteWorkflow();

        #endregion

        #region private methods
        /// <summary>
        /// Process task - approval is not required if current user is a member of the approving group
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="userId"></param>
        private void ApproveOrContinue(WorkflowTaskInstancePoco taskInstance, int userId)
        {
            if (IsStepApprovalRequired(taskInstance))
            {
                InitiateApprovalTask(taskInstance);
                GetDb().Update(taskInstance);
            }
            else
            {
                Instance.Status = (int)WorkflowStatus.PendingApproval;
                taskInstance.Status = (int)TaskStatus.NotRequired;
                taskInstance.Comment = taskInstance.Comment + " (APPROVAL AT STAGE " + (taskInstance.ApprovalStep + 1) + " NOT REQUIRED)";
                GetDb().Update(taskInstance);
                GetDb().Update(Instance);

                ActionWorkflow(Instance, WorkflowAction.Approve, userId, string.Empty);
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
            var taskInstance = Instance.TaskInstances.FirstOrDefault(ti => ti.TaskStatus == TaskStatus.PendingApproval || ti.TaskStatus == TaskStatus.NotRequired );
            if (taskInstance == null) return;

            EmailType? emailType = null;
            var emailRequired = false;

            switch (action)
            {
                case WorkflowAction.Approve:
                    taskInstance.Status = (int) TaskStatus.Approved;
                    break;

                case WorkflowAction.Reject:
                    Instance.Status = (int) WorkflowStatus.Rejected;
                    Instance.CompletedDate = DateTime.Now;
                    taskInstance.Status = (int) TaskStatus.Rejected;
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
                Notifications.Send(Instance, emailType.Value);
            }

            GetDb().Update(taskInstance);
        }

        /// <summary>
        /// Generate the next approval flow task, returning the new task and a bool indicating whether the publish action should becompleted (ie, this is the end of the flow)
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="approvalRequired"></param>
        /// <returns></returns>
        private WorkflowTaskInstancePoco CreateApprovalTask(int nodeId, out bool approvalRequired)
        {
            var taskInstance =
                new WorkflowTaskInstancePoco(TaskType.Approve)
                {
                    ApprovalStep = Instance.TaskInstances.Count,
                    WorkflowInstanceGuid = Instance.Guid,
                    Comment = Instance.AuthorComment
                };
            Instance.TaskInstances.Add(taskInstance);
            SetApprovalGroup(taskInstance, nodeId);
            approvalRequired = IsStepApprovalRequired(taskInstance);

            GetDb().Insert(taskInstance);            

            return taskInstance;
        }

        /// <summary>
        /// Find the next approval group where the current user and change author are not members
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="nodeId"></param>
        private void SetApprovalGroup(WorkflowTaskInstancePoco taskInstance, int nodeId)
        {
            var approvalGroup = Pr.PermissionsForNode(nodeId, 0);
            UserGroupPermissionsPoco group = null;

            if (approvalGroup.Any())
            {
                // approval group length will match the number of groups mapped to the node
                // only interested in the one that corresponds with the index of the most recently added workflow task
                group = approvalGroup.First(g => g.Permission == taskInstance.ApprovalStep);
                SetInstanceTotalSteps(approvalGroup.Count);
            }
            else
            {
                // Recurse up the tree until we find something
                var node = Utility.GetNode(nodeId);
                if (node.Level != 1)
                {
                    SetApprovalGroup(taskInstance, node.Parent.Id);
                }
                else // no group set, check for content-type approval then fallback to default approver
                {
                    var contentTypeApproval = Pr.PermissionsForNode(nodeId, Instance.Node.ContentType.Id).Where(g => g.ContentTypeId != 0).ToList();
                    if (contentTypeApproval.Any())
                    {
                        group = contentTypeApproval.First(g => g.Permission == taskInstance.ApprovalStep);
                        SetInstanceTotalSteps(approvalGroup.Count);
                    }
                    else
                    {
                        group = GetDb().Fetch<UserGroupPermissionsPoco>(SqlHelpers.UserGroupBasic, Pr.GetSettings().DefaultApprover).First();
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
        //private bool CheckSubsequentStep(List<UserGroupPermissionsPoco> approvalGroup, UserGroupPermissionsPoco group, int currentUserId, int groupIndex)
        //{
        //    var doPublish = false;
        //    // check the last permission is equal to the current group, and current user or instance author are members of that group, if so, the request should be published
        //    if (approvalGroup.OrderBy(g => g.Permission).Last().Permission == groupIndex && (group.UserGroup.IsMember(currentUserId) || group.UserGroup.IsMember(Instance.AuthorUserId)))
        //    {
        //        doPublish = true;
        //    }
        //    else
        //    {
        //        // if not the last group, check that the current user or instance author aren't a member of all the subsequent groups
        //        // if we find a group where the current user or instance author aren't a member, the approval flow continues
        //        // if they are part of all subsequent groups, the workflow should advance automatically
        //        // this does mean the current user could be in group 2 and 4, but not 3, so they will recieve a publish request for stage 4
        //        foreach (var g in approvalGroup.Where(g => g.Permission >= groupIndex))
        //        {
        //            if (g.UserGroup.IsMember(Instance.AuthorUserId) || g.UserGroup.IsMember(currentUserId))
        //            {
        //                doPublish = true;
        //            }
        //            else
        //            {
        //                doPublish = false;
        //                break;
        //            }
        //        }
        //    }
        //    return doPublish;
        //}

        /// <summary>
        /// Determines whether approval is required by checking if the Author is in the current task group.
        /// </summary>
        /// <returns>true if approval required, false otherwise</returns>
        private bool IsStepApprovalRequired(WorkflowTaskInstancePoco taskInstance)
        {
            return Utility.GetSettings().FlowType == (int)FlowType.All || (!taskInstance.UserGroup.IsMember(Instance.AuthorUserId) && !taskInstance.UserGroup.IsMember(Utility.GetCurrentUser().Id));
        }

        /// <summary>
        /// set the total steps property for a workflow instance
        /// </summary>
        /// <param name="stepCount">The number of approval groups in the current flow (explicit, inherited or content type)</param>
        private void SetInstanceTotalSteps(int stepCount)
        {
            if (Instance.TotalSteps != stepCount)
            {
                Instance.TotalSteps = stepCount;
                GetDb().Update(Instance);
            }
        }

        private void InitiateApprovalTask(WorkflowTaskInstancePoco taskInstance)
        {
            // Set task and workflow information.
            taskInstance.Status = (int)TaskStatus.PendingApproval;
            Instance.Status = (int)WorkflowStatus.PendingApproval;

            Notifications.Send(Instance, EmailType.ApprovalRequest);
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
