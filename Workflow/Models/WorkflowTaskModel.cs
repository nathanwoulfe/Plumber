
using System.Collections.Generic;

namespace Workflow.Models
{
    public class WorkflowInstance
    {
        public string Type { get; set; }
        public string NodeName { get; set; }
        public string Status { get; set; }
        public string CssStatus { get; set; }
        public string RequestedOn { get; set; }
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
        public string Type { get; set; }
        public string Status { get; set; }
        public string CssStatus { get; set; }
        public int NodeId { get; set; }
        public int TaskId { get; set; }
        public int ApprovalGroupId { get; set; }
        public int CurrentStep { get; set; }
        public string NodeName { get; set; }
        public string RequestedBy { get; set; }
        public string RequestedOn { get; set; }
        public string Comments { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedOn { get; set; }
        public string ApprovalComment { get; set; }
        public string ApprovalGroup { get; set; }
        public bool ShowActionLink { get; set; }
        public string ActiveTask { get; set; }
        public List<UserGroupPermissionsPoco> Permissions { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class WorkflowResponseItem
    {
        public string Message { get; set; }
        public WorkflowType Type { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class DifferencesResponseItem
    {
        public string CurrentVersionPubDate { get; set; }
        public string RevisedVersionPubDate { get; set; }
        public string CompareData { get; set; }
        public string CompareMessage { get; set; }
    }
}

