using System;

namespace Workflow.Helpers
{
    [Serializable]
    internal class WorkflowException : Exception
    {
        public WorkflowException(string message) : base(message) { }
    }

    /// <summary>
    /// Occurs when an Umbraco Operation (Publish or UnPublish) attempt fails without throwing an exception
    /// Treated as a 'friendly' exception with different behavior from unhandled exceptions, or exceptions contained in the operation Attempt
    /// </summary>
    [Serializable]
    internal class UmbracoOperationFailedException : Exception
    {
        public UmbracoOperationFailedException(string message) : base(message) { }
    }
}
