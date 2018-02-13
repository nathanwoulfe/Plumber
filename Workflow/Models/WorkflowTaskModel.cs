
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
        public DateTime? CompletedOn { get; set; }
        public DateTime RequestedOn { get; set; }
        public string RequestedBy { get; set; }
        public int NodeId { get; set; }        
        public List<WorkflowTask> Tasks { get; set; }

        public WorkflowInstance()
        {
            Tasks = new List<WorkflowTask>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WorkflowTask
    {
        public int NodeId { get; set; }
        public int TaskId { get; set; }
        public int ApprovalGroupId { get; set; }
        public int CurrentStep { get; set; }
        public int RequestedById { get; set; }

        public string Type { get; set; }
        public string Status { get; set; }
        public string CssStatus { get; set; }
        public string NodeName { get; set; }
        public string RequestedBy { get; set; }
        public string RequestedOn { get; set; }
        public string Comments { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedOn { get; set; }
        public string ApprovalComment { get; set; }
        public string ApprovalGroup { get; set; }
        public string ActiveTask { get; set; }

        public List<UserGroupPermissionsPoco> Permissions { get; set; }

        public Guid InstanceGuid { get; set; }
    }
}

