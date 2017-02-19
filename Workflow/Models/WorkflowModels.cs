
namespace Workflow.Models
{
    public enum TaskStatus
    {
        New = 1,
        Approved = 2,
        Rejected = 3,
        PendingApproval = 4,
        NotRequired = 5,
        Cancelled = 6
    }

    public enum TaskType
    {
        CoordinatorApproval = 1,
        FinalApproval = 2
    }

    /// <summary>
    /// The allowed actions that can be performed on a workflow by a user.
    /// </summary>
    public enum WorkflowAction
    {
        Approve = 1,
        Reject = 2,
        Cancel = 3
    }

    /// <summary>
    /// The status of a workflow instance.
    /// </summary>
    public enum WorkflowStatus
    {
        New = 1,
        PendingCoordinatorApproval = 2,
        PendingFinalApproval = 3,
        Rejected = 4,
        Cancelled = 5,
        Completed = 6,
        Errored = 7
    }

    /// <summary>
    /// The type of workflow that this instance is for.
    /// </summary>
    public enum WorkflowType
    {
        Publish = 1,
        Unpublish = 2
    }

    /// <summary>
    /// The types of emails that can be sent as part of a workflow.
    /// </summary>
    public enum EmailType
    {
        CoordinatorApprovalRequest = 1,
        CoordinatorApprovalRejection = 2,
        FinalApprovalRequest = 3,
        FinalApprovalRejection = 4,
        ApprovedAndCompleted = 5,
        ApprovedAndCompletedForScheduler = 6,
        SchedulerActionCancelled = 7,
        WorkflowCancelled = 8,
    }
    
}
