using Workflow.Models;

namespace Workflow.Events.Args
{
    public class InstanceEventArgs : EventArgsBase
    {
        public InstanceEventArgs(WorkflowInstancePoco instance, string type = "")
        {
            Instance = instance;
            Type = type;
        }

        public string Type { get; set; }
        public WorkflowInstancePoco Instance { get; set; }
    }
}
