using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chauffeur.TestingTools;
using GDev.Umbraco.Test;
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
            Scaffold.AddTables();

            var _mocker = new ContextMocker();
        }

        [Fact]
        public void Can_Initiate_Publish_Workflow()
        {
            Scaffold.AddContent();

            var process = new DocumentPublishProcess();
            WorkflowInstancePoco instance = process.InitiateWorkflow(1073, 3, "A test comment");

            Assert.NotNull(instance);
        }
    }
}
