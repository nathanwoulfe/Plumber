using System.Collections.Generic;
using System.Linq;
using Chauffeur.TestingTools;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Services
{
    public class ImportExportServiceTests : UmbracoHostTestBase
    {
        private readonly IImportExportService _importExportService;
        private readonly UmbracoContext _context;

        public ImportExportServiceTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.Tables();

            _context = Scaffold.EnsureContext();

            _importExportService = new ImportExportService();
        }

        [Fact]
        public void Can_Get_Service()
        {
            Assert.NotNull(_importExportService);
        }

        [Fact]
        public async void Can_Export_All()
        {
            Scaffold.Config();

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