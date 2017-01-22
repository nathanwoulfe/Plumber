using System.Collections.Generic;
using System.Linq;
using Workflow.Models;

namespace Workflow
{
    public static class Extensions
    {
        public static IEnumerable<UserGroupPoco> Active(this IQueryable<UserGroupPoco> groups)
        {
            return groups.Where(ug => ug.Deleted == false);
        }

        public static bool GroupNameExists(this IQueryable<UserGroupPoco> groups, string name)
        {
            return (groups.Any(ug => ug.Name == name));
        }

        public static bool GroupAliasExists(this IQueryable<UserGroupPoco> groups, string alias)
        {
            return (groups.Any(ug => ug.Alias == alias));
        }

        /// <summary>
        /// Determines the tasks that are pending approval for a list of user groups
        /// </summary>
        /// <param name="taskInstances">All task instances</param>
        /// <param name="userUserGroups">The user groups that a user belongs to.</param>
        /// <returns>The tasks currently pending approval that the user group is allocated to.</returns>
        public static IEnumerable<WorkflowTaskInstancePoco> ApprovalTasksForUserGroups(this List<WorkflowTaskInstancePoco> taskInstances, List<User2UserGroupPoco> userUserGroups)
        {
            List<WorkflowTaskInstancePoco> myTasks = new List<WorkflowTaskInstancePoco>();
            foreach (var uug in userUserGroups)
            {
                myTasks.AddRange(taskInstances.Where(ti => ti.GroupId == uug.GroupId && ti.Status == (int)TaskStatus.PendingApproval));
            }
            return myTasks;
        }
        //DBContext results are retrieved as list to force an enumeration on the collection to cause the LINQ code to run immediately before dbcontext disposal refer to https://jira.usc.edu.au/browse/WWW-2258  
        /// <summary>
        /// Determines if a document has an active workflow associated with it.
        /// </summary>
        /// <param name="instances">all workflow instances</param>
        /// <param name="docId">The id of the document</param>
        /// <returns></returns>
        public static bool HasActiveWorkflow(this List<WorkflowInstancePoco> instances, int docId)
        {
            return instances.Active().Any(wi => wi.NodeId == docId);
        }

        /// <summary>
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="docId">The id of the document to check.</param>
        /// <returns>The active workflow for a document or null if no workflow currently associated with a document.</returns>
        public static WorkflowInstancePoco ActiveWorkflow(this List<WorkflowInstancePoco> instances, int docId)
        {
            return instances.Active().FirstOrDefault(wi => wi.NodeId == docId);
        }

        public static List<WorkflowInstancePoco> Active(this List<WorkflowInstancePoco> instances)
        {
            return instances.Where(wi =>
                wi._Status != WorkflowStatus.Cancelled
                && wi._Status != WorkflowStatus.Completed
                && wi._Status != WorkflowStatus.Errored
                && wi._Status != WorkflowStatus.Rejected).ToList();
        }

        /// <summary>
        /// The workflow history for a document.
        /// </summary>
        /// <param name="instances">all instances</param>
        /// <param name="docId">The id of the document to get the workflow history for.</param>
        /// <returns>The most recent XXX workflow instances that are not currently active ordered by creation date desc</returns>
        public static IQueryable<WorkflowInstancePoco> WorkflowHistory(this IQueryable<WorkflowInstancePoco> instances, int docId)
        {
            return instances.Where(wi => wi.NodeId == docId 
                && wi._Status != WorkflowStatus.PendingCoordinatorApproval 
                && wi._Status != WorkflowStatus.PendingFinalApproval)
                .OrderByDescending(wi => wi.CreatedDate)
                .Take(10);
        }      
    }
}
