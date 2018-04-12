using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Chauffeur.TestingTools;
using GDev.Umbraco.Test;
using Workflow.Api;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Groups
{
    public class GroupControllerTests : UmbracoHostTestBase
    {
        private readonly GroupsController _groupsController;
        private readonly IGroupService _groupService;

        public GroupControllerTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.AddTables();

            // not testing this, but need it for quickly creating groups
            _groupService = new GroupService();

            var mocker = new ContextMocker();
            _groupsController = new GroupsController(mocker.UmbracoContextMock)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        [Fact]
        public async void Can_Create_Group()
        {
            string name = Utility.RandomString();

            var model = new Model
            {
                Data = name
            };

            dynamic content = await (await _groupsController.Post(model)).GetContent();

            Assert.Equal(200, Utility.ObjectValue(content, "status"));
            Assert.Equal(MagicStrings.GroupCreated.Replace("{name}", name), Utility.ObjectValue(content, "msg"));
        }

        // CAN DELETE VIA SERVICE - THIS REQUIRES GETCURRENTUSER TO WORK. NEED TO MOCK IT
        //[Fact]
        //public async void Can_Delete_Group()
        //{
        //    var model = new Model
        //    {
        //        Data = Utility.RandomString()
        //    };

        //    //create the group
        //    dynamic content = await (await _groupsController.Post(model)).GetContent();

        //    // delete the group using the returned group id
        //    dynamic result = await (await _groupsController.Delete((int)Utility.ObjectValue(content, "id"))).GetContent();

        //    Assert.Equal(200, Utility.ObjectValue(content, "status"));
        //    Assert.Equal(MagicStrings.GroupDeleted, Utility.ObjectValue(content, "msg"));
        //}

        [Fact]
        public async void Cannot_Create_Group_With_No_Name()
        {
            var model = new Model
            {
                Data = null
            };

            dynamic content = await (await _groupsController.Post(model)).GetContent();

            Assert.NotNull(Utility.ObjectValue(content, "ExceptionMessage"));
        }

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
            dynamic content = await (await _groupsController.Post(model)).GetContent();
           
            Assert.Equal(200, Utility.ObjectValue(content, "status"));
            Assert.Equal(MagicStrings.GroupNameExists, Utility.ObjectValue(content, "msg"));
        }

        [Fact]
        public async void Can_Get_Group_By_Id()
        {
            string name = Utility.RandomString();

            var model = new Model
            {
                Data = name
            };

            // create a group
            dynamic content = await (await _groupsController.Post(model)).GetContent();

            // fetch the group using the id returned on creation
            IHttpActionResult group = await _groupsController.Get((int)Utility.ObjectValue(content, "id"));
            JsonResult<UserGroupPoco> groupData = group as JsonResult<UserGroupPoco>;

            Assert.NotNull(groupData);
            Assert.Equal(name, groupData.Content.Name);
        }

        [Fact]
        public async void Get_Group_By_Invalid_Id_Will_Throw()
        {
            dynamic content = await (await _groupsController.Get(int.MaxValue)).GetContent();

            Assert.NotNull(content);
            Assert.Equal(MagicStrings.HttpResponseException, Utility.ObjectValue(content, "ExceptionType"));
            Assert.Equal(MagicStrings.ErrorGettingGroup.Replace("{id}", int.MaxValue.ToString()), Utility.ObjectValue(content, "ExceptionMessage"));
        }

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
