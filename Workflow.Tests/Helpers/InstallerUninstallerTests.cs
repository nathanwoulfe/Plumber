using Chauffeur.TestingTools;
using Umbraco.Core;
using Umbraco.Core.Services;
using Workflow.Helpers;
using Xunit;

namespace Workflow.Tests.Helpers
{
    public class InstallerUninstallerTests : UmbracoHostTestBase
    {
        private readonly ISectionService _sectionService;

        public InstallerUninstallerTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            _sectionService = ApplicationContext.Current.Services.SectionService;
        }

        [Fact]
        public void Can_Add_Section()
        {
            // delete it if it exists, before running installer
            Uninstaller.RemoveSection();

            Assert.Null(_sectionService.GetByAlias("workflow"));

            Assert.True(Installer.AddSection(ApplicationContext.Current));
        }

        /// <summary>
        /// Running both dash installers in the same method to not corrupt the config for dev
        /// </summary>
        [Fact]
        public void Can_Add_Dashboards()
        {
            Uninstaller.RemoveSectionDashboard();
            Assert.True(Installer.AddSectionDashboard());
            Assert.True(Installer.AddContentSectionDashboard());

            // installing when dashboards exist will fail
            Assert.False(Installer.AddSectionDashboard());
            Assert.False(Installer.AddContentSectionDashboard());
        }
    }
}

