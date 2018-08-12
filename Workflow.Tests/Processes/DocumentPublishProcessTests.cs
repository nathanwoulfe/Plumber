using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chauffeur.TestingTools;
using Umbraco.Core.Services;
using Workflow.Models;
using Workflow.Processes;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Processes
{
    public class DocumentPublishProcessTests : UmbracoHostTestBase
    {
        public DocumentPublishProcessTests()
        {
            Host.Run(new[] {"install y"}).Wait();

            Scaffold.Run();
        }

        [Fact]
        public void Can_Publish_Now()
        {
            WorkflowInstancePoco instance = Scaffold.Instance(Guid.NewGuid(), 1);

            var process = new DocumentPublishProcess();
            
        }
    }
}