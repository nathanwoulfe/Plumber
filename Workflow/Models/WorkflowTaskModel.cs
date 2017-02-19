
namespace Workflow.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkflowItem
    {
        public string Type { get; set; }
        public int NodeId { get; set; }
        public int TaskId { get; set; }
        public string RequestedBy { get; set; }
        public string RequestedOn { get; set; }
        public string Comments { get; set; }
        public string CoordinatedBy { get; set; }
        public string CoordinatedOn { get; set; }
        public string CoordinatorComments { get; set; }
        public string ApprovalGroup { get; set; }
        public bool ShowActionLink { get; set; }
        public string ActiveTask { get; set; }
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

