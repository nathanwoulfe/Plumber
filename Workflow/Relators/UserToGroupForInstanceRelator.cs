using System.Linq;
using Workflow.Models;

namespace Workflow.Relators
{
    internal class UserToGroupForInstanceRelator
    {
        private WorkflowInstancePoco _current;

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
                return _current;
            }

            if (ugp.GroupId == wtip.GroupId)
            {
                wtip.UserGroup = ugp;
            }

            if (_current != null && _current.Guid == wip.Guid)
            {
                if (_current.TaskInstances.All(t => t.ApprovalStep != wtip.ApprovalStep))
                {
                    _current.TaskInstances.Add(wtip);
                }
                return null;
            }

            var prev = _current;
            _current = wip;
            _current.TaskInstances.Add(wtip);

            return prev;
        }
    }
}