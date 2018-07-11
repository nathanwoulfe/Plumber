using Chauffeur.TestingTools;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Xunit;

namespace Workflow.Tests.Helpers
{
    public class InstallerTests : UmbracoHostTestBase
    {
        private readonly ISectionService _sectionService;

        public InstallerTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.Run();

            _sectionService = ApplicationContext.Current.Services.SectionService;
        }

        [Fact]
        public void Can_Add_Section()
        {
            // delete it if it exists, before running installer
            Section section = _sectionService.GetByAlias("workflow");
            if (section != null)
            {
                _sectionService.DeleteSection(section);
            }

            Assert.Null(_sectionService.GetByAlias("workflow"));

            var install = new Workflow.Helpers.Installer();
            Assert.True(install.AddSection(ApplicationContext.Current));
        }

        [Fact]
        public void Can_Add_Section_Dashboard()
        {
            var install = new Workflow.Helpers.Installer();
            Assert.True(install.AddSectionDashboard());
        }

        [Fact]
        public void Can_Add_Content_Section_Dashboard()
        {
            var install = new Workflow.Helpers.Installer();
            Assert.True(install.AddContentSectionDashboard());
        }
    }
}

