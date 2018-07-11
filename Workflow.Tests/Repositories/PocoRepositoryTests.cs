using Chauffeur.TestingTools;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Xunit;

namespace Workflow.Tests.Repositories
{
    public class PocoRepositoryTests : UmbracoHostTestBase
    {
        private readonly IPocoRepository _repo;

        public PocoRepositoryTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();

            _repo = new PocoRepository();
        }

        [Fact]
        public void Returns_True_If_Group_Alias_Exists()
        {
            Scaffold.Config();
            Assert.True(_repo.GroupAliasExists("publisher"));
        }

        [Fact]
        public void Returns_False_If_Group_Alias_Does_Not_Exist()
        {
            Scaffold.Config();
            Assert.False(_repo.GroupAliasExists("doesntexist"));
        }

        [Fact]
        public void Returns_Settings_If_Settings_Exist()
        {
            Scaffold.Config();
            WorkflowSettingsPoco settings = _repo.GetSettings();

            Assert.NotNull(settings);
            Assert.Equal("12", settings.DefaultApprover);
        }

        [Fact]
        public void Returns_New_Settings_If_No_Settings_Exist()
        {
            WorkflowSettingsPoco settings = _repo.GetSettings();

            // email will be populated automatically
            Assert.NotNull(settings);
            Assert.NotNull(settings.Email);
        }

        [Fact]
        public void Can_Update_Settings()
        {
            WorkflowSettingsPoco settings = _repo.GetSettings();
            string defaultApprover = settings.DefaultApprover;

            settings.DefaultApprover = "15";

            _repo.UpdateSettings(settings);

            settings = _repo.GetSettings();

            Assert.NotEqual(defaultApprover, settings.DefaultApprover);
        }

        [Fact]
        public void Can_Check_Node_Has_Permissions()
        {
            Scaffold.Config();

            Assert.True(_repo.NodeHasPermissions(1079));
        }
    }
}
