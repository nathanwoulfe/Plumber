using System;
using System.Collections.Generic;

namespace Workflow.Models
{
    public class WorkflowInstance
    {
        public string Type { get; set; }
        public string NodeName { get; set; }
        public string Status { get; set; }
        public string CssStatus { get; set; }
        public string Comment { get; set; }
        public string RequestedBy { get; set; }
        public string RequestedOn { get; set; }
        public int NodeId { get; set; }
        public List<WorkflowTask> Tasks { get; set; }

        // need these as a datetime for the charts, to match task model
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid InstanceGuid { get; set; }

        public WorkflowInstance()
        {
            Tasks = new List<WorkflowTask>();
        }
    }
}
