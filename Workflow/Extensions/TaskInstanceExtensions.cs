using System;
using Workflow.Models;

namespace Workflow.Extensions
{
    public static class TaskInstanceExtensions
    {
        public static EmailType? ProcessApproval(this WorkflowTaskPoco taskInstance, WorkflowAction action, int userId, string comment)
        {
            EmailType? emailAction = null;

            switch (action)
            {
                case WorkflowAction.Approve:
                    if (taskInstance.TaskStatus != TaskStatus.NotRequired)
                    {
                        taskInstance.Status = (int)TaskStatus.Approved;
                        emailAction = EmailType.ApprovalRequest;
                    }

                    break;

                case WorkflowAction.Reject:
                    taskInstance.Status = (int)TaskStatus.Rejected;
                    emailAction = EmailType.ApprovalRejection;

                    break;
            }

            taskInstance.CompletedDate = DateTime.Now;
            taskInstance.Comment = comment;
            taskInstance.ActionedByUserId = userId;

            taskInstance.ActionedByAdmin = ActionedByAdmin(taskInstance, userId);

            return emailAction;
        }

        /// <summary>
        /// Set the appropriate properties to indicate the task has been cancelled
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="userId"></param>
        /// <param name="reason"></param>
        /// <param name="completedDate"></param>
        public static void Cancel(this WorkflowTaskPoco taskInstance, int userId, string reason, DateTime? completedDate)
        {
            taskInstance.Status = (int)TaskStatus.Cancelled;
            taskInstance.ActionedByUserId = userId;
            taskInstance.Comment = reason;
            taskInstance.CompletedDate = completedDate;

            taskInstance.ActionedByAdmin = ActionedByAdmin(taskInstance, userId);
        }

        /// <summary>
        /// Create simple html markup for an inactive workflow task.
        /// </summary>
        /// <param name="taskInstance">The task instance.</param>
        /// <param name="index"></param>
        /// <returns>HTML markup describing an active task instance.</returns>
        public static string BuildTaskSummary(this WorkflowTaskPoco taskInstance, bool first)
        {
            var result = "";

            switch (taskInstance.Status)
            {
                case (int)TaskStatus.PendingApproval:
                case (int)TaskStatus.Approved:
                case (int)TaskStatus.Rejected:
                case (int)TaskStatus.Resubmitted:
                case (int)TaskStatus.Cancelled:
       
                    if (first)
                    {
                        result += $"<b>Stage {taskInstance.ApprovalStep + 1}</b>: {taskInstance.StatusName}";
                    }
 
                    if (taskInstance.CompletedDate != null)
                    {
                        result += $"<br/><br/>{taskInstance.StatusName} by {taskInstance.ActionedByUser.Name} on {taskInstance.CompletedDate.Value:d MMM yyyy} at {taskInstance.CompletedDate.Value:h:mmtt}";
                    }

                    if (taskInstance.Comment.HasValue())
                    {
                        result += $"<br/>&nbsp;&nbsp;Comment: <i>{taskInstance.Comment}</i>";
                    }

                    break;

                case (int)TaskStatus.NotRequired:

                    result += $"Stage {taskInstance.ApprovalStep + 1}: Not required";

                    break;
            }

            return result;
        }

        /// <summary>
        /// check if user is a member of the group, or is acting as an admin, then set flag
        /// If the usergroup doesn't exist, just return false - if that's the case, we have bigger problems...
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static bool ActionedByAdmin(WorkflowTaskPoco taskInstance, int userId)
        {
            return !taskInstance.UserGroup?.UsersSummary.Contains($"|{userId}|") ?? false;
        }
    }
}
