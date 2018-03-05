using System.Collections.Generic;
using System.Linq;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Extensions
{
    public static class WorkflowListExtensions
    {
        private static readonly IConfigService ConfigService = new ConfigService();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskInstances"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static List<WorkflowTask> ToWorkflowTaskList(this List<WorkflowTaskInstancePoco> taskInstances, WorkflowInstancePoco instance = null)
        {
            List<WorkflowTask> workflowItems = new List<WorkflowTask>();

            if (!taskInstances.Any()) return workflowItems;

            bool useInstanceFromTask = instance == null;

            foreach (WorkflowTaskInstancePoco taskInstance in taskInstances)
            {
                instance = useInstanceFromTask ? taskInstance.WorkflowInstance : instance;
 
                string instanceNodeName = instance.Node?.Name ?? "NODE NO LONGER EXISTS";
                string typeDescription = instance.TypeDescription;

                var item = new WorkflowTask
                {
                    Status = taskInstance.StatusName,
                    CssStatus = taskInstance.StatusName.ToLower().Split(' ')[0],
                    Type = typeDescription,
                    NodeId = instance.NodeId,
                    InstanceGuid = instance.Guid,
                    ApprovalGroupId = taskInstance.UserGroup.GroupId,
                    NodeName = instanceNodeName,
                    RequestedBy = instance.AuthorUser.Name,
                    RequestedById = instance.AuthorUser.Id,
                    RequestedOn = taskInstance.CreatedDate.ToString(),
                    ApprovalGroup = taskInstance.UserGroup.Name,
                    Comments = useInstanceFromTask ? instance.AuthorComment : taskInstance.Comment,
                    ActiveTask = taskInstance.StatusName,
                    Permissions = ConfigService.GetRecursivePermissionsForNode(instance.Node),
                    CurrentStep = taskInstance.ApprovalStep
                };

                workflowItems.Add(item);
            }

            return workflowItems.OrderByDescending(x => x.CurrentStep).ToList();            
        }

        /// <summary>
        /// Helper method for compiling WorkflowItem response object
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public static List<WorkflowInstance> ToWorkflowInstanceList(this List<WorkflowInstancePoco> instances)
        {
            List<WorkflowInstance> workflowInstances = new List<WorkflowInstance>();

            if (instances != null && instances.Count > 0)
            {
                foreach (var instance in instances)
                {
                    var model = new WorkflowInstance
                    {
                        Type = instance.TypeDescription,
                        Status = instance.StatusName,
                        CssStatus = instance.StatusName.ToLower().Split(' ')[0],
                        NodeId = instance.NodeId,
                        NodeName = instance.Node.Name,
                        RequestedBy = instance.AuthorUser.Name,
                        RequestedOn = instance.CreatedDate,
                        CompletedOn = instance.CompletedDate,
                        Tasks = instance.TaskInstances.ToList().ToWorkflowTaskList(instance)
                    };

                    workflowInstances.Add(model);
                }
            }

            return workflowInstances.OrderByDescending(x => x.RequestedOn).ToList();
        }
    }
}