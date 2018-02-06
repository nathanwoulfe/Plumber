using System;

namespace Workflow.Models
{
    public class InitiateWorkflowModel
    {
        public string NodeId { get; set; }
        public string Comment { get; set; }
        public bool Publish { get; set; }
    }

    // simple model for posting string value
    public class Model
    {
        public string Data { get; set; }
    }

    // simple model for posting to workflow task endpoints
    public class TaskData
    {
        public string Comment { get; set; }
        public int TaskId { get; set; }
        public Guid InstanceGuid { get; set; }
    }
}
