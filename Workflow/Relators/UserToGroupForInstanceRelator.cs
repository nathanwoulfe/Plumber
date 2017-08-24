using System.Linq;
using Workflow.Models;

namespace Workflow.Relators
{
    internal class UserToGroupForInstanceRelator
    {
        public WorkflowInstancePoco Current;

        /// <summary>
        /// Maps Users to the UserGroup property of a WorkflowTaskInstance
        /// </summary>
        /// <param name="wtip"></param>
        /// <param name="wip"></param>
        /// <param name="ugp"></param>
        /// <returns></returns>
        public WorkflowInstancePoco MapIt(WorkflowInstancePoco wip, WorkflowTaskInstancePoco wtip, UserGroupPoco ugp)
        {
            if (wip == null)
            {
                return Current;
            }

            if (ugp.GroupId == wtip.GroupId)
            {
                wtip.UserGroup = ugp;
            }

            if (Current != null && Current.Guid == wip.Guid)
            {
                if (Current.TaskInstances.All(t => t.ApprovalStep != wtip.ApprovalStep))
                {
                    Current.TaskInstances.Add(wtip);
                }
                return null;
            }

            var prev = Current;
            Current = wip;
            Current.TaskInstances.Add(wtip);

            return prev;
        }
    }
}