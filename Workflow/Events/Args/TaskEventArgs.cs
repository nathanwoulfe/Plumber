using Workflow.Models;

namespace Workflow.Events.Args
{
    public class TaskEventArgs : EventArgsBase
    {
        public TaskEventArgs(WorkflowTaskInstancePoco task)
        {
            Task = task;
        }

        public WorkflowTaskInstancePoco Task { get; set; }
    }
}