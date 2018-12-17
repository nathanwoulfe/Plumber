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
        Errored = 6,
        Resubmitted = 7
    }
}
