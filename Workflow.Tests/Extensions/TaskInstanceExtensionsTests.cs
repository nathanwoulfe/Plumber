using System;
using Workflow.Models;
using Xunit;

using TaskStatus = Workflow.Models.TaskStatus;

namespace Workflow.Tests.Extensions
{
    public class TaskInstanceExtensionsTests
    {
        [Fact]
        public void Can_Process_Task()
        {
            var taskInstance = new WorkflowTaskInstancePoco
            {
                WorkflowInstanceGuid = Guid.NewGuid(),
                ApprovalStep = 1,
                CreatedDate = DateTime.Now,
                Status = (int) TaskStatus.PendingApproval
            };
        }
    }
}
