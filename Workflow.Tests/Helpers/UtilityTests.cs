using Chauffeur.TestingTools;
using GDev.Umbraco.Test;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
using Umbraco.Web.Security;
using Workflow.Repositories;
using Xunit;

namespace Workflow.Tests.Helpers
{
    public class UtilityTests : UmbracoHostTestBase
    {
        private readonly Workflow.Helpers.Utility _utility;

        public UtilityTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.AddTables();

            var mocker = new ContextMocker();

            _utility = new Workflow.Helpers.Utility(
                new PocoRepository(),
                ApplicationContext.Current.Services.UserService,
                ApplicationContext.Current.Services.ContentTypeService,
                ApplicationContext.Current.Services.ContentService,
                UmbracoContext.Current);
        }

        [Fact]
        public void Can_Get_Node()
        {
            var content = ApplicationContext.Current.Services.ContentService.CreateContent("testNode", -1, "TextPage");

            object node = _utility.GetContent(1073);

            Assert.NotNull(node);
            Assert.IsAssignableFrom<IContent>(node);
        }

        [Fact]
        public void Can_Get_User_By_Id()
        {
            IUser user = _utility.GetUser(120);
            Assert.NotNull(user);
        }

        [Fact]
        public void Can_Get_Current_User()
        {
            var id = Utility.RandomInt();

            Mock<WebSecurity> webSecurity = new Mock<WebSecurity>(null, null);
            var currentUser = Mock.Of<IUser>(u =>
                u.IsApproved
                && u.Name == Utility.RandomString()
                && u.Id == id);

            webSecurity.Setup(x => x.CurrentUser).Returns(currentUser);

            IUser user = _utility.GetCurrentUser();
            Assert.NotNull(user);
            Assert.Equal(id, user.Id);
        }
    }
}
