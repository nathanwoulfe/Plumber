using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chauffeur.TestingTools;
using GDev.Umbraco.Test;
using Umbraco.Core;
using Umbraco.Core.Models;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;
using TaskStatus = Workflow.Models.TaskStatus;

namespace Workflow.Tests.Services
{
    public class PreviewServiceTests : UmbracoHostTestBase
    {
        private readonly IPreviewService _previewService;
        private readonly ITasksService _tasksService;
        private readonly IInstancesService _instancesService;
        private readonly IConfigService _configService;
        private readonly IGroupService _groupService;

        private ContextMocker _mocker;

        public PreviewServiceTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();
            Scaffold.Config();

            // even though it's not being used, this needs to stay
            _mocker = new ContextMocker();

            _previewService = new PreviewService();
            _tasksService = new TasksService();
            _instancesService = new InstancesService();
            _configService = new ConfigService();
            _groupService = new GroupService();
        }

        private async Task<UserGroupPoco> AddGroupWithPermissionAndUser(int userId, int nodeId)
        {
            // add a group with a user
            UserGroupPoco group = await _groupService.CreateUserGroupAsync("Test Group");
            group.Users.Add(new User2UserGroupPoco
            {
                GroupId = group.GroupId,
                UserId = userId
            });

            await _groupService.UpdateUserGroupAsync(group);

            // give the group permissions to the node
            var poco = new UserGroupPermissionsPoco
            {
                GroupId = group.GroupId,
                Permission = 0,
                NodeId = nodeId
            };

            _configService.UpdateNodeConfig(new Dictionary<int, List<UserGroupPermissionsPoco>>
            {
                { 0, new List<UserGroupPermissionsPoco> { poco } }
            });

            return group;
        }

        [Fact]
        public void Can_Get_Service()
        {
            Assert.NotNull(_previewService);
        }

        [Fact]
        public async void Can_Validate_Request()
        {
            Guid guid = Guid.NewGuid();

            const int userId = 11;
            const int nodeId = 1089;

            UserGroupPoco group = await AddGroupWithPermissionAndUser(userId, nodeId);

            // create a task on an instance
            WorkflowTaskPoco task = Scaffold.Task(guid, groupId: group.GroupId);

            _tasksService.InsertTask(task);
            _instancesService.InsertInstance(Scaffold.Instance(guid, 1, nodeId));

            // is valid when the user is in the group responsible for the task with the given id
            // and the task belongs to the given instance by guid
            // and both the task and instance are related to the given node id
            bool isValid = await _previewService.Validate(nodeId, userId, task.Id, guid);
            Assert.True(isValid);

            // invalid user id
            isValid = await _previewService.Validate(nodeId, 99, task.Id, guid);
            Assert.False(isValid);

            // invalid task id
            isValid = await _previewService.Validate(nodeId, userId, 11111, guid);
            Assert.False(isValid);

            // invalid guid
            isValid = await _previewService.Validate(nodeId, userId, task.Id, Guid.NewGuid());
            Assert.False(isValid);

            // invalid node id
            isValid = await _previewService.Validate(43535, userId, task.Id, guid);
            Assert.False(isValid);
        }

        [Fact]
        public async void Cannot_Validate_Request_When_Last_Task_Not_Pending()
        {
            Guid guid = Guid.NewGuid();

            const int userId = 446;
            const int nodeId = 3456;

            UserGroupPoco group = await AddGroupWithPermissionAndUser(userId, nodeId);

            // create a task on an instance
            WorkflowTaskPoco task = Scaffold.Task(guid, groupId: group.GroupId, status: (int)TaskStatus.NotRequired);

            _tasksService.InsertTask(task);
            _instancesService.InsertInstance(Scaffold.Instance(guid, 1, nodeId));

            bool isValid = await _previewService.Validate(nodeId, userId, task.Id, guid);
            Assert.False(isValid);
        }

        [Fact]
        public async void Cannot_Validate_Request_When_No_Tasks()
        {
            Guid guid = Guid.NewGuid();

            const int userId = 46;
            const int nodeId = 34904;

            _instancesService.InsertInstance(Scaffold.Instance(guid, 1, nodeId));

            bool isValid = await _previewService.Validate(nodeId, userId, 6456, guid);
            Assert.False(isValid);
        }

        [Fact]
        public void Can_Generate_Preview()
        {
            Scaffold.ContentType(ApplicationContext.Current.Services.ContentTypeService);
            IContent node = Scaffold.Node(ApplicationContext.Current.Services.ContentService);
            Guid guid = Guid.NewGuid();

            _instancesService.InsertInstance(Scaffold.Instance(guid, 0, node.Id));

            _previewService.Generate(node.Id, 0, guid);
        }
    }
}