using Workflow.Models;
using Workflow.Extensions;
using Xunit;

namespace Workflow.Tests.Extensions
{
    public class EnumExtensionsTests
    {
        [Theory]
        [InlineData(new object[] {TaskStatus.Approved, TaskStatus.Errored, TaskStatus.PendingApproval}, TaskStatus.Approved, true)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Errored, TaskStatus.NotRequired },TaskStatus.Cancelled, false)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Cancelled, TaskStatus.PendingApproval }, TaskStatus.Cancelled, true)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Resubmitted, TaskStatus.PendingApproval }, TaskStatus.Rejected, false)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Errored, TaskStatus.Rejected }, TaskStatus.Resubmitted, false)]
        public void TaskStatus_In_Set_Returns_True(object[] data, TaskStatus? status, bool result)
        {
            Assert.Equal(result, status.In(data));
        }

        [Theory]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Errored, TaskStatus.PendingApproval }, TaskStatus.Approved, false)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Errored, TaskStatus.NotRequired }, TaskStatus.Cancelled, true)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Cancelled, TaskStatus.PendingApproval }, TaskStatus.Cancelled, false)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Resubmitted, TaskStatus.PendingApproval }, TaskStatus.Rejected, true)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Errored, TaskStatus.Rejected }, TaskStatus.Resubmitted, true)]
        public void TaskStatus_Not_In_Set_Returns_True(object[] data, TaskStatus? status, bool result)
        {
            Assert.Equal(result, status.NotIn(data));
        }

        [Theory]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Errored, WorkflowStatus.PendingApproval }, WorkflowStatus.Approved, true)]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Errored, WorkflowStatus.NotRequired }, WorkflowStatus.Cancelled, false)]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Cancelled, WorkflowStatus.PendingApproval }, WorkflowStatus.Cancelled, true)]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Resubmitted, WorkflowStatus.PendingApproval }, WorkflowStatus.Rejected, false)]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Errored, WorkflowStatus.Rejected }, WorkflowStatus.Resubmitted, false)]
        public void WorkflowStatus_In_Set_Returns_True(object[] data, WorkflowStatus status, bool result)
        {
            Assert.Equal(result, status.In(data));
        }

        [Theory]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Errored, WorkflowStatus.PendingApproval }, WorkflowStatus.Approved, false)]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Errored, WorkflowStatus.NotRequired }, WorkflowStatus.Cancelled, true)]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Cancelled, WorkflowStatus.PendingApproval }, WorkflowStatus.Cancelled, false)]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Resubmitted, WorkflowStatus.PendingApproval }, WorkflowStatus.Rejected, true)]
        [InlineData(new object[] { WorkflowStatus.Approved, WorkflowStatus.Errored, WorkflowStatus.Rejected }, WorkflowStatus.Resubmitted, true)]
        public void WorkflowStatus_Not_In_Set_Returns_True(object[] data, WorkflowStatus status, bool result)
        {
            Assert.Equal(result, status.NotIn(data));
        }
    }
}
