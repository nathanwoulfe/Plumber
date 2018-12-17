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
        /// <param name="task"></param>
        /// <param name="instance"></param>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public WorkflowInstancePoco MapIt(WorkflowInstancePoco instance, WorkflowTaskPoco task, UserGroupPoco userGroup)
        {
            if (instance == null)
            {
                return _current;
            }

            if (userGroup.GroupId == task.GroupId)
            {
                task.UserGroup = userGroup;
            }

            if (_current != null && _current.Guid == instance.Guid)
            {
                if (_current.TaskInstances.All(t => t.ApprovalStep != task.ApprovalStep))
                {
                    _current.TaskInstances.Add(task);
                }
                return null;
            }

            WorkflowInstancePoco prev = _current;
            _current = instance;
            _current.TaskInstances.Add(task);

            return prev;
        }
    }
}