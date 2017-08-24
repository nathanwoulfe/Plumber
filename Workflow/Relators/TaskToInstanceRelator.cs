using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Relators
{
    public class TaskToInstanceRelator
    {
        public WorkflowInstancePoco Current;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public WorkflowInstancePoco MapIt(WorkflowInstancePoco a, WorkflowTaskInstancePoco b)
        {
            if (a == null)
            {
                return Current;
            }

            if (Current != null && a.Guid == b.WorkflowInstanceGuid)
            {
                Current.TaskInstances.Add(b);
                return null;
            }

            var prev = Current;
            Current = a;
            Current.TaskInstances = new List<WorkflowTaskInstancePoco>() { b };

            return prev;
        }
    }
}
