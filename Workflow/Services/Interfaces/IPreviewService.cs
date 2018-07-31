using System;
using System.Threading.Tasks;

namespace Workflow.Services.Interfaces
{
    /// <summary>
    /// Service for manipulating preview sets for offline approvals
    /// </summary>
    public interface IPreviewService
    {
        void Generate(int nodeId, int userId, Guid workflowInstanceGuid);

        Task<bool> Validate(int nodeId, int userId, int taskId, Guid workflowInstanceGuid);
    }
}
