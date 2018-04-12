using Workflow.Models;

namespace Workflow.Processes
{
    internal interface IWorkflowProcess
    {
        WorkflowInstancePoco InitiateWorkflow(int nodeId, int authorUserId, string authorComment);
        WorkflowInstancePoco ResubmitWorkflow(WorkflowInstancePoco instance, int userId, string comment);
        WorkflowInstancePoco ActionWorkflow(WorkflowInstancePoco instance, WorkflowAction action, int userId, string comment);
        WorkflowInstancePoco CancelWorkflow(WorkflowInstancePoco instance, int userId, string reasons);
    }
}
