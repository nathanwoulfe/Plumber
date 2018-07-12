using System.Collections.Generic;
using Chauffeur.TestingTools;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Xunit;

namespace Workflow.Tests.Repositories
{
    /// <summary>
    /// Import is tested implicitly in Scaffold.Config
    /// </summary>
    public class ImportExportRepositoryTests : UmbracoHostTestBase
    {
        private readonly IImportExportRepository _repo;

        public ImportExportRepositoryTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();

            _repo = new ImportExportRepository();
        }

        [Fact]
        public void Can_Export_Settings()
        {
            WorkflowSettingsExport settings = _repo.ExportSettings();
            Assert.NotNull(settings);
            Assert.Null(settings.DefaultApprover);

            Scaffold.Config();
            settings = _repo.ExportSettings();
            Assert.NotNull(settings);
            Assert.Equal("12", settings.DefaultApprover);
        }

        [Fact]
        public void Can_Export_User2UserGroups()
        {
            IEnumerable<User2UserGroupExport> user2UserGroups = _repo.ExportUser2UserGroups();
            Assert.Empty(user2UserGroups);

            Scaffold.Config();

            user2UserGroups = _repo.ExportUser2UserGroups();
            Assert.NotEmpty(user2UserGroups);
        }

        [Fact]
        public void Can_Export_UserGroups()
        {
            IEnumerable<UserGroupExport> userGroups = _repo.ExportUserGroups();
            Assert.Empty(userGroups);

            Scaffold.Config();

            userGroups = _repo.ExportUserGroups();
            Assert.NotEmpty(userGroups);
        }

        [Fact]
        public void Can_Export_UserGroupPermissions()
        {
            IEnumerable<UserGroupPermissionsExport> userGroupPermissions = _repo.ExportUserGroupPermissions();
            Assert.Empty(userGroupPermissions);

            Scaffold.Config();

            userGroupPermissions = _repo.ExportUserGroupPermissions();
            Assert.NotEmpty(userGroupPermissions);
        }
    }
}
