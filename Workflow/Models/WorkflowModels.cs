namespace Workflow.Models
{
    /// <summary>
    /// The status of a workflow task instance
    /// </summary>
    public enum TaskStatus
    {
        Approved = 1,
        Rejected = 2,
        PendingApproval = 3,
        NotRequired = 4,
        Cancelled = 5,
        Errored = 6
    }

    /// <summary>
    /// The permitted task action types
    /// </summary>
    public enum TaskType
    {
        Approve = 1,
        Publish = 2
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
        Approved = 1,
        Rejected = 2,
        PendingApproval = 3,
        NotRequired = 4,
        Cancelled = 5,
        Errored = 6
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
        ApprovalRequest = 1,
        ApprovalRejection = 2,
        ApprovedAndCompleted = 3,
        ApprovedAndCompletedForScheduler = 4,
        SchedulerActionCancelled = 5,
        WorkflowCancelled = 6,
    }    
}
