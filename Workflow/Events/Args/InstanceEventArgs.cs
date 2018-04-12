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

        private string Type { get; set; }
        private WorkflowInstancePoco Instance { get; set; }
    }
}
