using Chauffeur.TestingTools;
using System;
using Workflow.Extensions;
using Workflow.Models;
using Xunit;

using TaskStatus = Workflow.Models.TaskStatus;

namespace Workflow.Tests.Extensions
{
    public class TaskInstanceExtensionsTests : UmbracoHostTestBase
    {
        public TaskInstanceExtensionsTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.Run();
        }

        [Theory]
        [InlineData(WorkflowAction.Approve, 0, "comment text", EmailType.ApprovalRequest)]
        public void Can_Process_Task(WorkflowAction action, int userId, string comment, EmailType expected)
        {
            var taskInstance = new WorkflowTaskInstancePoco
            {
                WorkflowInstanceGuid = Guid.NewGuid(),
                ApprovalStep = 1,
                CreatedDate = DateTime.Now,
                Status = (int) TaskStatus.PendingApproval
            };

            EmailType? emailType = taskInstance.ProcessApproval(action, userId, comment);

            Assert.Equal(expected, emailType.Value);
        }
    }
}
