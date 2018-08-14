using Chauffeur.TestingTools;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Services
{
    public class ImportExportServiceTests : UmbracoHostTestBase
    {
        private readonly IImportExportService _importExportService;

        public ImportExportServiceTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();

            _importExportService = new ImportExportService();
        }

        [Fact]
        public void Can_Get_Service()
        {
            Assert.NotNull(_importExportService);
        }

        [Fact]
        public async void Can_Import_Config()
        {
            var model = Scaffold.ReadFromJsonFile<ImportExportModel>(@"Config.json");
            bool import = await _importExportService.Import(model);

            Assert.True(import);
        }

        [Fact]
        public async void Can_Export_All()
        {
            var model = Scaffold.ReadFromJsonFile<ImportExportModel>(@"Config.json");
            bool import = await _importExportService.Import(model);

            Assert.True(import);

            ImportExportModel export = await _importExportService.Export();

            Assert.NotNull(export);
            Assert.NotEmpty(export.User2UserGroup);
            Assert.NotEmpty(export.UserGroupPermissions);
            Assert.NotEmpty(export.UserGroups);
        }

        [Fact]
        public async void Export_Empty_If_No_Config()
        {
            ImportExportModel export = await _importExportService.Export();

            Assert.NotNull(export);
            Assert.Empty(export.User2UserGroup);
            Assert.Empty(export.UserGroupPermissions);
            Assert.Empty(export.UserGroups);
        }
    }
}