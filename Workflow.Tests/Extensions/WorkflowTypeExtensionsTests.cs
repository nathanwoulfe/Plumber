using System;
using Workflow.Extensions;
using Workflow.Models;
using Xunit;

namespace Workflow.Tests.Extensions
{
    public class WorkflowTypeExtensionsTests
    {
        [Theory]
        [InlineData(WorkflowType.Publish, null, "Publish")]
        [InlineData(WorkflowType.Publish, 2, "Schedule for Publish")]
        [InlineData(WorkflowType.Unpublish, null, "Unpublish")]
        [InlineData(WorkflowType.Unpublish, 3, "Schedule for Unpublish")]
        public void Can_Get_Type_Description(WorkflowType type, int? days, string expected)
        {
            DateTime? date = null;
            if (days.HasValue)
            {
                date = DateTime.Now.AddDays(days.Value);
            }

            string description = type.Description(date);

            Assert.StartsWith(expected, description);
        }

        [Theory]
        [InlineData(WorkflowType.Publish, null, "Published")]
        [InlineData(WorkflowType.Publish, 2, "Scheduled to be Published")]
        [InlineData(WorkflowType.Unpublish, null, "Unpublished")]
        [InlineData(WorkflowType.Unpublish, 3, "Scheduled to be Unpublished")]
        public void Can_Get_Past_Tense_Type_Description(WorkflowType type, int? days, string expected)
        {
            DateTime? date = null;
            if (days.HasValue)
            {
                date = DateTime.Now.AddDays(days.Value);
            }

            string description = type.DescriptionPastTense(date);

            Assert.StartsWith(expected, description);
        }
    }
}
