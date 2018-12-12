using System;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using Chauffeur.TestingTools;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Workflow.Api;
using Workflow.Models;
using Xunit;
using Constants = Workflow.Helpers.Constants;

namespace Workflow.Tests.Api
{
    public class SettingsControllerTests : UmbracoHostTestBase
    {
        private readonly SettingsController _settingsController;

        public SettingsControllerTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            Scaffold.Run();

            _settingsController = new SettingsController
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        [Fact]
        public async void Can_Get_Version()
        {
            dynamic result = await _settingsController.GetVersion().GetContent();
            Assert.NotNull(result);

            // get from cache
            dynamic result2 = await _settingsController.GetVersion().GetContent();
            Assert.NotNull(result2);
        }

        [Fact]
        public async void Get_Generic_Error_If_Docs_Unavailable()
        {
            MemoryCache cache = MemoryCache.Default;
            cache[Constants.VersionKey] = Utility.RandomString();

            dynamic result = await _settingsController.GetVersion().GetContent();
            Assert.NotNull(result);
            Assert.Equal(Constants.ErrorGettingVersion, result);
        }

        [Fact]
        public void Can_Get_Docs()
        {
            HttpResponseMessage result = _settingsController.GetDocs();
            Assert.NotNull(result);
            Assert.Equal(200, (int)result.StatusCode);
            Assert.IsAssignableFrom<StringContent>(result.Content);
        }

        [Fact]
        public void Can_Get_Docs_From_Cache()
        {
            MemoryCache cache = MemoryCache.Default;
            cache.Add(Constants.DocsKey, "A string", new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddHours(6) });

            HttpResponseMessage result = _settingsController.GetDocs();
            Assert.NotNull(result);
            Assert.Equal(200, (int)result.StatusCode);
            Assert.IsAssignableFrom<StringContent>(result.Content);
        }

        [Fact]
        public async void Can_Get_Settings()
        {
            JObject result = await _settingsController.Get().GetContent();
            Assert.NotNull(result);

            // no config has been scaffolded, so settings should be defaults
            Assert.Equal(1, result["id"]);
            Assert.Equal(0, result["flowType"]);
        }

        [Fact]
        public async void Can_Update_Settings()
        {
            var model = new WorkflowSettingsPoco
            {
                DefaultApprover = "12",
                EditUrl = "some.url.com",
                Email = "some.email@mail.com",
                ExcludeNodes = "",
                FlowType = 1,
                SendNotifications = true,
                SiteUrl = "site.url.com"
            };

            string result = await _settingsController.Save(model).GetContent();
            Assert.Equal(Constants.SettingsUpdated, result);
        }

        [Fact]
        public async void Can_Get_ContentTypes()
        {
            JArray result = await _settingsController.GetContentTypes().GetContent();
            Assert.NotNull(result);

            Scaffold.ContentType(ApplicationContext.Current.Services.ContentTypeService, "TestType");
            Scaffold.ContentType(ApplicationContext.Current.Services.ContentTypeService, "AnotherType");

            result = await _settingsController.GetContentTypes().GetContent();
            Assert.Equal(2, result.Count);
            Assert.Equal("TestType", result[0]["name"]);

        }
    }
}
