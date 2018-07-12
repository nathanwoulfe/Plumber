using System.Collections.Generic;
using System.Linq;
using Chauffeur.TestingTools;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;
using WorkflowUtility = Workflow.Helpers.Utility;

namespace Workflow.Tests.Helpers
{
    public class UtilityTests : UmbracoHostTestBase
    {
        private readonly WorkflowUtility _utility;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IConfigService _configService;

        public UtilityTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();

            _contentService = ApplicationContext.Current.Services.ContentService;
            _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            _configService = new ConfigService();

            _utility = new WorkflowUtility(
                new PocoRepository(),
                ApplicationContext.Current.Services.UserService,
                _contentTypeService,
                _contentService,
                UmbracoContext.Current);
        }

        [Fact]
        public void Can_Get_Node()
        {
            Scaffold.ContentType(_contentTypeService);
            IContent home = Scaffold.Node(_contentService);

            object node = _utility.GetContent(home.Id);

            Assert.NotNull(node);
            Assert.Equal(home.Id, node.Get("Id"));
            Assert.IsAssignableFrom<IContent>(node);
        }

        [Fact]
        public void Can_Get_Node_Name()
        {
            Scaffold.ContentType(_contentTypeService);
            IContent home = Scaffold.Node(_contentService);

            string name = _utility.GetNodeName(home.Id);

            Assert.NotNull(name);
            Assert.Equal(home.Name, name);
        }

        [Fact]
        public void Can_Get_Content_Type()
        {
            Scaffold.ContentType(_contentTypeService);
            IEnumerable<IContentType> contentTypes = _contentTypeService.GetAllContentTypes();
            IContentType textpage = contentTypes.First(x => x.Alias == "textpage");

            IContentType result = _utility.GetContentType(textpage.Id);

            Assert.Equal(textpage.Id, result.Id);
        }

        [Fact]
        public void Can_Get_Ancestor_Permissions()
        {
            // scaffold
            Scaffold.ContentType(_contentTypeService);
            IContent root = Scaffold.Node(_contentService);
            IContent child = Scaffold.Node(_contentService, root.Id);
            IContent childChild = Scaffold.Node(_contentService, child.Id);

            // set permissions on root
            // mock some data
            Dictionary<int, List<UserGroupPermissionsPoco>> config = Scaffold.Permissions(root.Id, 3, 2);

            _configService.UpdateNodeConfig(config);

            bool hasFlow = _utility.HasFlow(childChild.Id);

            Assert.True(hasFlow);
        }

        [Fact]
        public void Can_Get_User_By_Id()
        {
            IUser user = _utility.GetUser(0);
            Assert.NotNull(user);
        }

        [Fact]
        public void Can_Get_Current_User()
        {
            int id = UmbracoContext.Current.Security.CurrentUser.Id;

            IUser user = _utility.GetCurrentUser();
            Assert.NotNull(user);
            Assert.Equal(id, user.Id);
        }

        [Theory]
        [InlineData("alias@domain.com.au", true)]
        [InlineData("this.is@valid", true)]
        [InlineData("thisisjusta-string", false)]
        public void Can_Validate_Email(string value, bool expected)
        {
            Assert.Equal(expected, _utility.IsValidEmailAddress(value));
        }

        [Theory]
        [InlineData("pascalCasedString", "Pascal Cased String")]
        [InlineData(null, null)]
        public void Can_Convert_String_Casing(string value, string expected)
        {
            // yes, strings can have other formats, but these are internal and I control the format
            Assert.Equal(expected, _utility.PascalCaseToTitleCase(value));
        }

    }
}
