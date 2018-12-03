using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Chauffeur.TestingTools;
using Umbraco.Web;
using Workflow.Api;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Api
{
    public class GroupControllerTests : UmbracoHostTestBase
    {
        private readonly GroupsController _groupsController;
        private readonly IGroupService _groupService;

        public GroupControllerTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();

            // not testing this, but need it for quickly creating groups
            _groupService = new GroupService();

            _groupsController = new GroupsController(UmbracoContext.Current)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        [Fact]
        public void Constructors_Work()
        {
            // chasing coverage - make sure constructors are all accessible
            Assert.NotNull(new GroupsController());
            Assert.NotNull(new GroupsController(UmbracoContext.Current, new UmbracoHelper(UmbracoContext.Current)));
        }

        [Fact]
        public async void Can_Create_Group()
        {
            string name = Utility.RandomString();

            var model = new Model
            {
                Data = name
            };

            object content = await (await _groupsController.Post(model)).GetContent();

            Assert.Equal(200, (int)content.Get("status"));
            Assert.Equal(Constants.GroupCreated.Replace("{name}", name), content.Get("msg"));
        }

        [Fact]
        public async void Can_Update_Group()
        {
            Scaffold.Config();

            var group = new UserGroupPoco
            {
                GroupId = 12,
                Name = "PublisherUpdated",
                Alias = "publisherUpdated",
                Users = new List<User2UserGroupPoco>()
            };

            object result = await (await _groupsController.Put(group)).GetContent();

            Assert.Equal(Constants.GroupUpdated.Replace("{name}", "PublisherUpdated"), result.Get("msg"));
        }

        [Fact]
        public async void Cannot_Update_Group_With_Invalid_Model()
        {
            Scaffold.Config();

            object result = await(await _groupsController.Put(null)).GetContent();

            Assert.Equal(Constants.ErrorUpdatingGroup, result.Get("ExceptionMessage"));
        }

        [Fact]
        public async void Cannot_Update_Group_If_Name_In_Use()
        {
            Scaffold.Config();

            var group = new UserGroupPoco
            {
                GroupId = 3,
                Name = "Publisher",
                Users = new List<User2UserGroupPoco>()
            };

            object result = await (await _groupsController.Put(group)).GetContent();

            Assert.Equal(Constants.GroupNameExists, result.Get("msg"));
        }

        [Fact]
        public async void Can_Delete_Group()
        {
            var model = new Model
            {
                Data = Utility.RandomString()
            };

            //create the group
            object content = await (await _groupsController.Post(model)).GetContent();

            // delete the group using the returned group id
            object result = await (await _groupsController.Delete((int)content.Get("id"))).GetContent();

            Assert.Equal(Constants.GroupDeleted, result);

            // what happens if the group doesnt exist?
            object result2 = await (await _groupsController.Delete(9999)).GetContent();

            // not a lot - task will succeed, but nothing is actually deleted. does this matter?
            Assert.NotNull(result2);
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async void Cannot_Create_Group_With_No_Name()
        {
            var model = new Model
            {
                Data = null
            };

            object content = await (await _groupsController.Post(model)).GetContent();

            Assert.NotNull(content.Get("ExceptionMessage"));
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async void Cannot_Create_Group_When_Name_Exists()
        {
            var model = new Model
            {
                Data = Utility.RandomString()
            };

            // create the first group
            await _groupsController.Post(model);

            // try again with the same name
            object content = await (await _groupsController.Post(model)).GetContent();

            Assert.Equal(200, content.Get("status"));
            Assert.Equal(Constants.GroupNameExists, content.Get("msg"));
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async void Can_Get_Group_By_Id()
        {
            string name = Utility.RandomString();

            var model = new Model
            {
                Data = name
            };

            // create a group
            object content = await (await _groupsController.Post(model)).GetContent();

            // fetch the group using the id returned on creation
            IHttpActionResult group = await _groupsController.Get((int)content.Get("id"));
            JsonResult<UserGroupPoco> groupData = group as JsonResult<UserGroupPoco>;

            Assert.NotNull(groupData);
            Assert.Equal(name, groupData.Content.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async void Get_Group_By_Invalid_Id_Will_Throw()
        {
            object content = await (await _groupsController.Get(int.MaxValue)).GetContent();

            Assert.NotNull(content);
            Assert.Equal(Constants.HttpResponseException, content.Get("ExceptionType"));
            Assert.Equal(Constants.ErrorGettingGroup.Replace("{id}", int.MaxValue.ToString()), content.Get("ExceptionMessage"));
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async void Can_Get_All_Groups()
        {
            // populate groups
            await _groupService.CreateUserGroupAsync(Utility.RandomString());
            await _groupService.CreateUserGroupAsync(Utility.RandomString());
            await _groupService.CreateUserGroupAsync(Utility.RandomString());
            await _groupService.CreateUserGroupAsync(Utility.RandomString());
            await _groupService.CreateUserGroupAsync(Utility.RandomString());

            IHttpActionResult group = await _groupsController.Get();
            JsonResult<IEnumerable<UserGroupPoco>> groupData = group as JsonResult<IEnumerable<UserGroupPoco>>;

            Assert.NotNull(groupData);
            Assert.NotNull(groupData.Content);
            Assert.Equal(5, groupData.Content.Count());
        }
    }
}
