using System;
using System.Linq;
using Umbraco.Core.Models;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;

using TaskStatus = Workflow.Models.TaskStatus;
using TaskType = Workflow.Models.TaskType;

namespace Workflow.Extensions
{
    public static class WorkflowInstanceExtensions
    {
        /// <summary>
        /// If the node associated with this workflow instance has a release or expiry date, set same on the workflow instance
        /// </summary>
        public static void SetScheduledDate(this WorkflowInstancePoco instance)
        {
            var utility = new Utility();

            IContent content = utility.GetContent(instance.NodeId);
            switch (instance.Type)
            {
                case (int)WorkflowType.Publish when content.ReleaseDate.HasValue:
                    instance.ScheduledDate = content.ReleaseDate;
                    break;
                case (int)WorkflowType.Unpublish when content.ExpireDate.HasValue:
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

            var instancesService = new InstancesService();
            instancesService.UpdateInstance(instance);
        }

        /// <summary>
        /// Adds an approval task to this workflow instance, setting the approval step and instance guid
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="taskInstance"></param>
        public static void CreateApprovalTask(this WorkflowInstancePoco instance, out WorkflowTaskInstancePoco taskInstance)
        {
            taskInstance = new WorkflowTaskInstancePoco(TaskType.Approve)
            {
                ApprovalStep = instance.TaskInstances.Count(x => x.TaskStatus.In(TaskStatus.Approved, TaskStatus.NotRequired)),
                WorkflowInstanceGuid = instance.Guid
            };

            instance.TaskInstances.Add(taskInstance);
        }
    }
}
