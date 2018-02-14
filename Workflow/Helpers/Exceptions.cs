using System;

namespace Workflow.Helpers
{
    [Serializable]
    internal class WorkflowException : Exception
    {
        public WorkflowException(string message) : base(message) { }
    }    
}
