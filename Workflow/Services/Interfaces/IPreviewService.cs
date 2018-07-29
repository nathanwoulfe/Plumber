using System;
using System.Xml;

namespace Workflow.Services.Interfaces
{
    /// <summary>
    /// Service for manipulating preview sets for offline approvals
    /// </summary>
    public interface IPreviewService
    {
        void Generate(int nodeId, Guid workflowInstanceGuid);
        void Delete(Guid workflowInstanceGuid);

        XmlDocument Fetch(Guid workflowInstanceGuid);
    }
}
