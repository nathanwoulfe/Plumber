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
        [InlineData(WorkflowAction.Reject, 1, "comment text", EmailType.ApprovalRejection)]
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

            Assert.Equal(comment, taskInstance.Comment);
            Assert.Equal(userId, taskInstance.ActionedByUserId);
        }

        [Theory]
        [InlineData(0, "comment text")]
        [InlineData(11, "second comment text")]
        public void Can_Cancel_Task(int userId, string comment)
        {
            var taskInstance = new WorkflowTaskInstancePoco
            {
                WorkflowInstanceGuid = Guid.NewGuid(),
                ApprovalStep = 1,
                CreatedDate = DateTime.Now.AddDays(-2),
                Status = (int)TaskStatus.PendingApproval
            };

            DateTime cancelledAt = DateTime.Now.AddDays(-1);

            taskInstance.Cancel(userId, comment, cancelledAt);

            Assert.Equal(comment, taskInstance.Comment);
            Assert.Equal(userId, taskInstance.ActionedByUserId);
            Assert.Equal(TaskStatus.Cancelled, taskInstance.TaskStatus);
        }

        [Theory]
        [InlineData(TaskStatus.Approved)]
        [InlineData(TaskStatus.Rejected)]
        [InlineData(TaskStatus.Cancelled)]
        [InlineData(TaskStatus.NotRequired)]
        public void Can_Get_Summary_String(TaskStatus status)
        {
            var taskInstance = new WorkflowTaskInstancePoco
            {
                WorkflowInstanceGuid = Guid.NewGuid(),
                ApprovalStep = 1,
                CreatedDate = DateTime.Now,
                Status = (int)status
            };

            string summary = taskInstance.BuildTaskSummary();

            Assert.NotNull(summary);
        }
    }
}
