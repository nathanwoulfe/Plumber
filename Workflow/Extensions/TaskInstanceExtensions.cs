using System;
using Workflow.Models;

namespace Workflow.Extensions
{
    public static class TaskInstanceExtensions
    {
        public static void ProcessApproval(this WorkflowTaskInstancePoco taskInstance, WorkflowAction action, int userId, string comment, out EmailType? emailAction)
        {
            emailAction = null;

            switch (action)
            {
                case WorkflowAction.Approve:
                    if (taskInstance.TaskStatus != TaskStatus.NotRequired)
                    {
                        taskInstance.Status = (int)TaskStatus.Approved;
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
        }

        /// <summary>
        /// Set the appropriate properties to indicate the task has been cancelled
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="userId"></param>
        /// <param name="reason"></param>
        /// <param name="completedDate"></param>
        public static void Cancel(this WorkflowTaskInstancePoco taskInstance, int userId, string reason, DateTime? completedDate)
        {
            taskInstance.Status = (int)TaskStatus.Cancelled;
            taskInstance.ActionedByUserId = userId;
            taskInstance.Comment = reason;
            taskInstance.CompletedDate = completedDate;
        }
    }
}
