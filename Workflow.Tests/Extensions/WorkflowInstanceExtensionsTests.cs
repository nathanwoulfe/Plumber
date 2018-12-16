using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chauffeur.TestingTools;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Processes;
using Xunit;

namespace Workflow.Tests.Extensions
{
    public class WorkflowInstanceExtensionsTests : UmbracoHostTestBase
    {
        public WorkflowInstanceExtensionsTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.Run();
        }

        [Theory]
        [InlineData(10, 2)]
        [InlineData(5, 5)]
        public void Can_Set_Total_Steps(int steps, int instanceSteps)
        {
            var instance = new WorkflowInstancePoco
            {
                TotalSteps = instanceSteps
            };

            instance.SetTotalSteps(steps);

            Assert.Equal(steps, instance.TotalSteps);
        }

        [Theory]
        [InlineData(WorkflowType.Publish, typeof(DocumentPublishProcess))]
        [InlineData(WorkflowType.Unpublish, typeof(DocumentUnpublishProcess))]
        public void Can_Get_Process(WorkflowType type, Type expected)
        {
            var instance = new WorkflowInstancePoco
            {
                Type = (int) type
            };

            WorkflowProcess process = instance.GetProcess();

            Assert.IsType(expected, process);
        }

        [Fact]
        public void Can_Cancel()
        {
            var instance = new WorkflowInstancePoco();

            instance.Cancel();

            Assert.NotNull(instance.CompletedDate);
            Assert.Equal(WorkflowStatus.Cancelled, instance.WorkflowStatus);
        }
    }
}
