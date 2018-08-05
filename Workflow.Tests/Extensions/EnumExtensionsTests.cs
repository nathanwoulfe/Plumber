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
        public void Value_In_Set_Returns_True(object[] data, TaskStatus? status, bool result)
        {
            Assert.Equal(result, status.In(data));
        }

        [Theory]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Errored, TaskStatus.PendingApproval }, TaskStatus.Approved, false)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Errored, TaskStatus.NotRequired }, TaskStatus.Cancelled, true)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Cancelled, TaskStatus.PendingApproval }, TaskStatus.Cancelled, false)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Resubmitted, TaskStatus.PendingApproval }, TaskStatus.Rejected, true)]
        [InlineData(new object[] { TaskStatus.Approved, TaskStatus.Errored, TaskStatus.Rejected }, TaskStatus.Resubmitted, true)]
        public void Value_Not_In_Set_Returns_True(object[] data, TaskStatus? status, bool result)
        {
            Assert.Equal(result, status.NotIn(data));
        }
    }
}
