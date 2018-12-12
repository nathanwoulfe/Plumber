using Chauffeur.TestingTools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Workflow.Api;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Api
{
    public class TasksControllerTests : UmbracoHostTestBase
    {
        private readonly TasksController _tasksController;
        private readonly ITasksService _tasksService;
        private readonly IInstancesService _instancesService;
        private readonly IConfigService _configService;

        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;

        public TasksControllerTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();

            _contentService = ApplicationContext.Current.Services.ContentService;
            _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            _tasksService = new TasksService();
            _instancesService = new InstancesService();
            _configService = new ConfigService();

            _tasksController = new TasksController
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        [Fact]
        public async void Get_Pending_Tasks_Response_Is_Generic_When_Id_Zero()
        {
            // get a  generic response if id is 0 
            object content = await _tasksController.GetNodePendingTasks(0).GetContent();
            Assert.Null(content.Get("settings"));
            Assert.Null(content.Get("noFlow"));
        }

        [Fact]
        public async void Get_Pending_Tasks_Response_Is_Generic_When_No_Settings_Or_Flow()
        {
            Scaffold.ContentType(_contentTypeService);
            IContent node = Scaffold.Node(_contentService);

            // generic response if no settings
            object content = await _tasksController.GetNodePendingTasks(node.Id).GetContent();
            Assert.Null(content.Get("settings"));
            Assert.Null(content.Get("noFlow"));
        }

        [Fact]
        public async void Get_Pending_Tasks_Response_Is_Zero_When_No_Node()
        {
            JObject content = await _tasksController.GetNodePendingTasks(666).GetContent();
            Assert.Equal(0, content.Value<int>("total"));
        }

        [Fact]
        public async void Can_Get_Node_Pending_Tasks()
        {
            Scaffold.ContentType(_contentTypeService);
            IContent node = Scaffold.Node(_contentService);

            Scaffold.Config();

            Guid guid = Guid.NewGuid();

            _instancesService.InsertInstance(Scaffold.Instance(guid, 1, node.Id));
            _tasksService.InsertTask(Scaffold.Task(guid));

            // needs flow or function exits
            Dictionary<int, List<UserGroupPermissionsPoco>> config = Scaffold.Permissions(node.Id, 3, 0);
            _configService.UpdateNodeConfig(config);

            JObject content = await _tasksController.GetNodePendingTasks(node.Id).GetContent();

            Assert.Single(content.Value<JArray>("items"));
        }

        [Fact]
        public async void Can_Get_Paged_Node_Tasks()
        {
            // get an error if the node doesn't exist
            //object response = await _tasksController.GetNodeTasks(666, -1, -1).GetContent();
            //Assert.Equal("NullReferenceException", (string)response.Get("ExceptionType"));
            //Assert.Equal(MagicStrings.ErrorGettingPendingTasksForNode.Replace("{id}", "666"), (string)response.Get("ExceptionMessage"));
            
            Scaffold.ContentType(_contentTypeService);
            IContent node = Scaffold.Node(_contentService);

            Scaffold.Config();

            Guid guid = Guid.NewGuid();

            _instancesService.InsertInstance(Scaffold.Instance(guid, 1, node.Id));
            _tasksService.InsertTask(Scaffold.Task(guid));
            _tasksService.InsertTask(Scaffold.Task(guid));
            _tasksService.InsertTask(Scaffold.Task(guid));

            // needs flow or function exits
            Dictionary<int, List<UserGroupPermissionsPoco>> config = Scaffold.Permissions(node.Id, 3, 2);
            _configService.UpdateNodeConfig(config);

            JObject content = await _tasksController.GetNodeTasks(node.Id, 10, 1).GetContent();

            Assert.Equal(1, content.Value<int>("totalPages"));
            Assert.Equal(10, content.Value<int>("count"));
            Assert.Equal(3, content.Value<JArray>("items").Count);

            // when 3 tasks, 1 per page, page 2 should be 1 item
            content = await _tasksController.GetNodeTasks(node.Id, 1, 2).GetContent();

            Assert.Equal(3, content.Value<int>("totalPages"));
            Assert.Equal(1, content.Value<int>("count"));
            Assert.Single(content.Value<JArray>("items"));

            // when 5 tasks, and 2 per page, page 2 should be 2 items
            _tasksService.InsertTask(Scaffold.Task(guid));
            _tasksService.InsertTask(Scaffold.Task(guid));

            content = await _tasksController.GetNodeTasks(node.Id, 2, 2).GetContent();

            Assert.Equal(3, content.Value<int>("totalPages"));
            Assert.Equal(2, content.Value<int>("count"));
            Assert.Equal(2, content.Value<JArray>("items").Count);
        }
    }
}
