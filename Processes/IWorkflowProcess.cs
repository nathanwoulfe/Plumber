using Workflow.Models;

namespace Workflow
{
    interface IWorkflowProcess
    {
        WorkflowInstancePoco InitiateWorkflow(int nodeId, int authorUserId, string authorComment);
        WorkflowInstancePoco ActionWorkflow(WorkflowInstancePoco instance, WorkflowAction action, int userId, string comment);
        WorkflowInstancePoco CancelWorkflow(WorkflowInstancePoco instance, int userId, string reasons);
    }
}
