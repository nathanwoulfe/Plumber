using Workflow.Models;

namespace Workflow.Events.Args
{
    public class TaskEventArgs : EventArgsBase
    {
        public TaskEventArgs(WorkflowTaskPoco task)
        {
            Task = task;
        }

        public WorkflowTaskPoco Task { get; set; }
    }
}