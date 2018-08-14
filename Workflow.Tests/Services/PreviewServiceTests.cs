using System;
using Chauffeur.TestingTools;
using GDev.Umbraco.Test;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Services
{
    public class PreviewServiceTests : UmbracoHostTestBase
    {
        private readonly IPreviewService _previewService;
        private readonly ITasksService _tasksService;
        private readonly IInstancesService _instancesService;
        private ContextMocker _mocker;

        public PreviewServiceTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();

            // even though it's not being used, this needs to stay
            _mocker = new ContextMocker();
            _previewService = new PreviewService();
            _tasksService = new TasksService();
            _instancesService = new InstancesService();
        }

        [Fact]
        public void Can_Get_Service()
        {
            Assert.NotNull(_previewService);
        }

        [Fact]
        public async void Can_Validate_Request()
        {
            Scaffold.Config();

            Guid guid = Guid.NewGuid();

            WorkflowTaskInstancePoco task = Scaffold.Task(guid);

            _tasksService.InsertTask(task);
            _instancesService.InsertInstance(Scaffold.Instance(guid, 1, 1089));

            // is valid when the user is in the group responsible for the task with the given id
            // and the task belongs to the given instance by guid
            // and both the task and instance are related to the given node id
            bool isValid = await _previewService.Validate(1089, 0, task.Id, guid);
            Assert.True(isValid);

            isValid = await _previewService.Validate(1089, 99, 6456, guid);
            Assert.False(isValid);
        }

        [Fact]
        public async void Cannot_Validate_Request_When_Last_Task_Not_Pending()
        {
            Scaffold.Config();

            Guid guid = Guid.NewGuid();

            WorkflowTaskInstancePoco task = Scaffold.Task(guid, status: (int)TaskStatus.NotRequired);

            _tasksService.InsertTask(task);
            _instancesService.InsertInstance(Scaffold.Instance(guid, 1, 1089));

            bool isValid = await _previewService.Validate(1089, 0, task.Id, guid);
            Assert.False(isValid);
        }

        [Fact]
        public async void Cannot_Validate_Request_When_No_Tasks()
        {
            Scaffold.Config();

            Guid guid = Guid.NewGuid();

            _instancesService.InsertInstance(Scaffold.Instance(guid, 1, 1089));

            bool isValid = await _previewService.Validate(1089, 99, 6456, guid);
            Assert.False(isValid);
        }
    }
}