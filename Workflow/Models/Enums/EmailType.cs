namespace Workflow.Models
{
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
        WorkflowErrored = 7
    }
}
