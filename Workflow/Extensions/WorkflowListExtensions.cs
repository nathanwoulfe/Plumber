using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Workflow.Models;
using Workflow.Repositories;

namespace Workflow.Extensions
{
    public static class WorkflowListExtensions
    {
        private static List<UserGroupPermissionsPoco> _perms = new List<UserGroupPermissionsPoco>();
        private static readonly PocoRepository Pr = new PocoRepository();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskInstances"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static List<WorkflowTask> ToWorkflowTaskList(this List<WorkflowTaskInstancePoco> taskInstances, WorkflowInstancePoco instance = null)
        {
            List<WorkflowTask> workflowItems = new List<WorkflowTask>();

            if (!taskInstances.Any()) return workflowItems.OrderByDescending(x => x.CurrentStep).ToList();

            bool useInstanceFromTask = instance == null;

            foreach (WorkflowTaskInstancePoco taskInstance in taskInstances)
            {
                instance = useInstanceFromTask ? taskInstance.WorkflowInstance : instance;
 
                GetPermissionsForNode(instance.Node);

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
                    Permissions = _perms,
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

        /// <summary>
        /// Get the explicit or implied approval flow for a given node
        /// </summary>
        private static void GetPermissionsForNode(IPublishedContent node)
        {
            // check the node for set permissions
            if (node == null) return;

            _perms = Pr.PermissionsForNode(node.Id, 0);

            // return them if they exist, otherwise check the parent
            if (!_perms.Any())
            {
                if (node.Level != 1)
                {
                    GetPermissionsForNode(node.Parent);
                }
                else
                {
                    // check for content-type permissions
                    _perms = Pr.PermissionsForNode(0, node.ContentType.Id);
                }
            }
        }
    }
}