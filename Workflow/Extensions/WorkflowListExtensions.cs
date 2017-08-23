using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Workflow.Models;

namespace Workflow.Extensions
{
    public static class WorkflowListExtensions
    {
        private static List<UserGroupPermissionsPoco> perms = new List<UserGroupPermissionsPoco>();
        private static PocoRepository _pr = new PocoRepository();

        public static List<WorkflowTask> ToWorkflowTaskList(this List<WorkflowTaskInstancePoco> taskInstances, WorkflowInstancePoco instance = null)
        {
            List<WorkflowTask> workflowItems = new List<WorkflowTask>();

            if (taskInstances != null && taskInstances.Count > 0)
            {
                foreach (var taskInstance in taskInstances)
                {
                    WorkflowInstancePoco useThisInstance = taskInstance.WorkflowInstance != null ? taskInstance.WorkflowInstance : instance;

                    var instanceNodeName = "NODE NO LONGER EXISTS";
                    var typeDescription = "";
                    if (useThisInstance.Node != null)
                    {
                        GetPermissionsForNode(useThisInstance.Node);
                        instanceNodeName = useThisInstance.Node.Name;
                        typeDescription = useThisInstance.TypeDescription;
                    }

                    var item = new WorkflowTask
                    {
                        Status = taskInstance.StatusName,
                        CssStatus = taskInstance.StatusName.ToLower().Split(' ')[0],
                        Type = typeDescription,
                        NodeId = useThisInstance.NodeId,
                        TaskId = useThisInstance.Id,
                        ApprovalGroupId = taskInstance.UserGroup.GroupId,
                        NodeName = instanceNodeName,
                        RequestedBy = useThisInstance.AuthorUser.Name,
                        RequestedOn = taskInstance.CreatedDate.ToString(),
                        ApprovalGroup = taskInstance.UserGroup.Name,
                        Comments = taskInstance.Comment != null ? taskInstance.Comment : useThisInstance.AuthorComment != null ? useThisInstance.AuthorComment : string.Empty,
                        ActiveTask = useThisInstance.StatusName,
                        Permissions = perms,
                        CurrentStep = taskInstance.ApprovalStep
                    };

                    workflowItems.Add(item);
                }
            }

            return workflowItems.OrderByDescending(x => x.CurrentStep).ToList();            
        }

        /// <summary>
        /// Helper method for compiling WorkflowItem response object
        /// </summary>
        /// <param name="taskInstances"></param>
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
            perms = _pr.PermissionsForNode(node.Id, 0);

            // return them if they exist, otherwise check the parent
            if (!perms.Any())
            {
                if (node.Level != 1)
                {
                    GetPermissionsForNode(node.Parent);
                }
                else
                {
                    // check for content-type permissions
                    perms = _pr.PermissionsForNode(0, node.ContentType.Id);
                }
            }
        }
    }
}