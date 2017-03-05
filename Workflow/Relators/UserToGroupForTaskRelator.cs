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
        public WorkflowTaskInstancePoco MapIt(WorkflowTaskInstancePoco wtip, WorkflowInstancePoco wip, UserGroupPoco a, User2UserGroupPoco b)
        {           
            if (wtip == null)
            {
                return current;
            }

            if (current != null && current.GroupId == wtip.GroupId) {
                if (!current.UserGroup.Users.Where(u => u.UserId == b.UserId).Any())
                {
                    current.UserGroup.Users.Add(b);
                }   
                return null;
            }

            var prev = current;
            current = wtip;
            current.WorkflowInstance = wip;
            current.UserGroup = a;
            current.UserGroup.Users.Add(b);

            return prev;
        }
    }
}
