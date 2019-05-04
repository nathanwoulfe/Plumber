using System;
using System.Linq;
using Umbraco.Core.Models;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Processes;

using TaskStatus = Workflow.Models.TaskStatus;
using TaskType = Workflow.Models.TaskType;

namespace Workflow.Extensions
{
    public static class WorkflowInstanceExtensions
    {
        /// <summary>
        /// If the node associated with this workflow instance has a release or expiry date, set same on the workflow instance
        /// THIS IS NOT PERSISTED
        /// </summary>
        public static void SetScheduledDate(this WorkflowInstancePoco instance)
        {
            var utility = new Utility();

            IContent content = utility.GetContent(instance.NodeId);
            switch (instance.Type)
            {
                case (int)WorkflowType.Publish when content?.ReleaseDate != null:
                    instance.ScheduledDate = content.ReleaseDate;
                    break;
                case (int)WorkflowType.Unpublish when content?.ExpireDate != null:
                    instance.ScheduledDate = content.ExpireDate;
                    break;
                default:
                    instance.ScheduledDate = null;
                    break;
            }
        }

        /// <summary>
        /// Set the instance properties to show it has been cancelled
        /// </summary>
        /// <param name="instance"></param>
        public static void Cancel(this WorkflowInstancePoco instance)
        {
            instance.CompletedDate = DateTime.Now;
            instance.Status = (int)WorkflowStatus.Cancelled;
        }

        /// <summary>
        /// set the total steps property for a workflow instance
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="steps">The number of approval groups in the current flow (explicit, inherited or content type)</param>
        public static void SetTotalSteps(this WorkflowInstancePoco instance, int steps)
        {
            if (instance.TotalSteps == steps) return;

            instance.TotalSteps = steps;
        }

        /// <summary>
        /// Adds an approval task to this workflow instance, setting the approval step and instance guid
        /// </summary>
        /// <param name="instance"></param>
        public static WorkflowTaskPoco CreateApprovalTask(this WorkflowInstancePoco instance)
        {
            var taskInstance = new WorkflowTaskPoco(TaskType.Approve)
            {
                ApprovalStep = instance.TaskInstances.Count(x => x.TaskStatus.In(TaskStatus.Approved, TaskStatus.NotRequired)),
                WorkflowInstanceGuid = instance.Guid
            };

            instance.TaskInstances.Add(taskInstance);

            return taskInstance;
        }

        /// <summary>
        /// Create the new process object for the instance - publish or unpublish
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static WorkflowProcess GetProcess(this WorkflowInstancePoco instance)
        {
            if ((WorkflowType)instance.Type == WorkflowType.Publish)
            {
                return new DocumentPublishProcess();
            }
            return new DocumentUnpublishProcess();
        }

        /// <summary>
        /// Builds workflow instance details markup.
        /// </summary>
        /// <returns>HTML tr inner html definition</returns>
        public static string BuildProcessSummary(this WorkflowInstancePoco instance)
        {
            string result = $"{instance.WorkflowType.Description(instance.ScheduledDate)} requested by {instance.AuthorUser.Name} on {instance.CreatedDate:dd/MM/yy} - {instance.StatusName}<br/>";

            if (instance.AuthorComment.HasValue())
            {
                result += $"&nbsp;&nbsp;Comment: <i>{instance.AuthorComment}</i>";
            }
            result += "<br/>";

            foreach (WorkflowTaskPoco taskInstance in instance.TaskInstances.OrderBy(t => t.ApprovalStep))
            {
                result += taskInstance.BuildTaskSummary() + "<br/>";
            }

            return $"{result}<br/>";
        }
    }
}
