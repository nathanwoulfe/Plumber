using Chauffeur.TestingTools;
using Workflow.Models;
using Workflow.Processes;
using Xunit;

namespace Workflow.Tests.Processes
{
    public class WorkflowProcessTests : UmbracoHostTestBase
    {
        public WorkflowProcessTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();
        }

        [Fact]
        public void Can_Initiate_Publish_Workflow()
        {
            Scaffold.Config();

            var process = new DocumentPublishProcess();
            WorkflowInstancePoco instance = process.InitiateWorkflow(1073, 3, "A test comment");

            Assert.NotNull(instance);
        }
    }
}
