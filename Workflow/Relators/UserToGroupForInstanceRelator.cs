using System.Linq;
using Workflow.Models;

namespace Workflow
{
    internal class UserToGroupForInstanceRelator
    {
        public WorkflowInstancePoco current;

        /// <summary>
        /// Maps Users to the UserGroup property of a WorkflowTaskInstance
        /// </summary>
        /// <param name="wtip"></param>
        /// <param name="wip"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public WorkflowInstancePoco MapIt(WorkflowInstancePoco wip, WorkflowTaskInstancePoco wtip, UserGroupPoco ugp)
        {
            if (wip == null)
            {
                return current;
            }

            if (ugp.GroupId == wtip.GroupId)
            {
                wtip.UserGroup = ugp;
            }

            if (current != null && current.Guid == wip.Guid)
            {
                if (!current.TaskInstances.Where(t => t.ApprovalStep == wtip.ApprovalStep).Any())
                {
                    current.TaskInstances.Add(wtip);
                }
                return null;
            }

            var prev = current;
            current = wip;
            current.TaskInstances.Add(wtip);

            return prev;
        }
    }
}