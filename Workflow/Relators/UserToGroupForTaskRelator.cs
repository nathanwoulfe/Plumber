using System.Linq;
using Workflow.Models;

namespace Workflow.Relators
{
    public class UserToGroupForTaskRelator
    {
        public WorkflowTaskInstancePoco current;

        /// <summary>
        /// Maps Users to the UserGroup property of a WorkflowTaskInstance
        /// </summary>
        /// <param name="wtip"></param>
        /// <param name="wip"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public WorkflowTaskInstancePoco MapIt(WorkflowTaskInstancePoco wtip, WorkflowInstancePoco wip, UserGroupPoco ugp)
        {           
            if (wtip == null)
            {
                return current;
            }

            if (ugp.GroupId == wtip.GroupId)
            {
                wtip.UserGroup = ugp;
            }

            if (current != null && current.GroupId == wtip.GroupId) {
                if (current.WorkflowInstance == null)
                {
                    current.WorkflowInstance = wip;
                }
                return null;
            }

            var prev = current;
            current = wtip;
            current.WorkflowInstance = wip;
            current.UserGroup = ugp;

            return prev;
        }
    }
}
