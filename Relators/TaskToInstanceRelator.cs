using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Relators
{
    public class TaskToInstanceRelator
    {
        public WorkflowInstancePoco current;

        public WorkflowInstancePoco MapIt(WorkflowInstancePoco a, WorkflowTaskInstancePoco b)
        {
            if (a == null)
            {
                return current;
            }

            if (a != null && current != null && a.Guid == b.WorkflowInstanceGuid)
            {
                current.TaskInstances.Add(b);
                return null;
            }

            var prev = current;
            current = a;
            current.TaskInstances = new List<WorkflowTaskInstancePoco>() { b };

            return prev;
        }
    }
}
