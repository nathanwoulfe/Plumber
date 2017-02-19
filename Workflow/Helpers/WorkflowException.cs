using System;

namespace Workflow
{
    class WorkflowException : Exception
    {
        public WorkflowException(string message) : base(message) { }
    }
}
