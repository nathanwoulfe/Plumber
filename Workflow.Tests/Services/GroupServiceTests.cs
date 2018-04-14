using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chauffeur.TestingTools;
using GDev.Umbraco.Test;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Services
{
    public class GroupServiceTests : UmbracoHostTestBase
    {
        private readonly IGroupService _groupService;

        private ContextMocker _mocker;

        public GroupServiceTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.AddTables();

            // even though it's not being used, this needs to stay
            _mocker = new ContextMocker();
            _groupService = new GroupService();

        }

        [Fact]
        public void Can_Get_Service()
        {
            Assert.NotNull(_groupService);
        }

        [Fact]
        public async void Can_Create_Group_And_Event_Is_Raised()
        {
            _groupService.Created += (sender, args) =>
            {
                Assert.NotNull(args);
            };

            UserGroupPoco newGroup = await _groupService.CreateUserGroupAsync(Utility.RandomString());

            Assert.NotNull(newGroup);
            Assert.True(newGroup.GroupId > 0);
        }

        [Fact]
        public async void Can_Update_Group_And_Event_Is_Raised()
        {
            _groupService.Updated += (sender, args) =>
            {
                Assert.NotNull(args);
            };

            const string description = "This is an update";
            const string email = "group@email.com";

            UserGroupPoco group = await _groupService.CreateUserGroupAsync(Utility.RandomString());
            group.Description = description;
            group.GroupEmail = email;

            UserGroupPoco updatedGroup = await _groupService.UpdateUserGroupAsync(group);

            Assert.Equal(description, updatedGroup.Description);
            Assert.Equal(email, updatedGroup.GroupEmail);
        }

        /// <summary>
        /// When a group is renamed, if the name is in use, the service should return null
        /// </summary>
        [Fact]
        public async void Cannot_Update_Group_When_Name_Exists()
        {
            string name = Utility.RandomString();
            string name2 = Utility.RandomString();

            // create two groups, second will be renamed to the first
            await _groupService.CreateUserGroupAsync(name);
            UserGroupPoco group = await _groupService.CreateUserGroupAsync(name2);

            group.Name = name;

            UserGroupPoco renamedGroup = await _groupService.UpdateUserGroupAsync(group);

            Assert.Null(renamedGroup);
        }

        /// <summary>
        /// User ID is mocked - we don't care if the user exists, just that we can update the Plumber table
        /// </summary>
        [Fact]
        public async void Can_Add_Users_To_Group()
        {
            string name = Utility.RandomString();

            UserGroupPoco group = await _groupService.CreateUserGroupAsync(name);

            group.Users.Add(Scaffold.GetUser2UserGroupPoco(group.GroupId));
            group.Users.Add(Scaffold.GetUser2UserGroupPoco(group.GroupId));
            group.Users.Add(Scaffold.GetUser2UserGroupPoco(group.GroupId));
            group.Users.Add(Scaffold.GetUser2UserGroupPoco(group.GroupId));

            UserGroupPoco updatedGroup = await _groupService.UpdateUserGroupAsync(group);

            Assert.Equal(4, updatedGroup.Users.Count());
        }

        [Fact]
        public async void Can_Get_Group_With_Users()
        {
            string name = Utility.RandomString();

            UserGroupPoco group = await _groupService.CreateUserGroupAsync(name);

            group.Users.Add(Scaffold.GetUser2UserGroupPoco(group.GroupId));
            group.Users.Add(Scaffold.GetUser2UserGroupPoco(group.GroupId));
            group.Users.Add(Scaffold.GetUser2UserGroupPoco(group.GroupId));

            await _groupService.UpdateUserGroupAsync(group);

            UserGroupPoco updatedGroup = await _groupService.GetPopulatedUserGroupAsync(group.GroupId);

            Assert.Equal(3, updatedGroup.Users.Count());
        }

        [Fact]
        public async void Can_Get_Group_By_Id()
        {
            // create a group
            UserGroupPoco newGroup = await _groupService.CreateUserGroupAsync(Utility.RandomString());
            // then use its id to fetch
            Assert.NotNull(await _groupService.GetUserGroupAsync(newGroup.GroupId));
        }

        [Fact]
        public async void Can_Delete_Group_And_Event_Is_Raised()
        {
            // populate groups
            Scaffold.AddContent();

            IEnumerable<UserGroupPoco> groups = await _groupService.GetUserGroupsAsync();
            int groupId = groups.First().GroupId;

            _groupService.Deleted += (sender, args) =>
            {
                Assert.NotNull(args);
                Assert.Equal(groupId, args.GroupId);
            };

            await _groupService.DeleteUserGroupAsync(groupId);
        }
    }
}