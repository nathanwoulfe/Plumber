using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Chauffeur.TestingTools;
using GDev.Umbraco.Test;
using Umbraco.Web;
using Workflow.Api;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Api
{
    public class TasksControllerTests : UmbracoHostTestBase
    {
        private readonly TasksController _tasksController;
        private readonly ContextMocker _mocker;
        private readonly ITasksService _tasksService;

        public TasksControllerTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.AddTables();

            _mocker = new ContextMocker();

            _tasksController = new TasksController(_mocker.UmbracoContextMock)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        [Fact]
        public void Constructors_Work()
        {
            // chasing coverage - make sure constructors are all accessible
            Assert.NotNull(new TasksController());
            Assert.NotNull(new TasksController(_mocker.UmbracoContextMock,
                new UmbracoHelper(_mocker.UmbracoContextMock)));
        }

    }
}
