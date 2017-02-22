namespace Workflow.Models
{
    public class InitiateWorkflowModel
    {
        public string NodeId { get; set; }
        public string Comment { get; set; }
        public bool Publish { get; set; }
    }
}
