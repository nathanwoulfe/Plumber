using Chauffeur.TestingTools;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Workflow.Extensions;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Extensions
{
    public class ContentExtensionsTests : UmbracoHostTestBase
    {
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;

        public ContentExtensionsTests()
        {
            Host.Run(new[] { "install y" }).Wait();
            Scaffold.Tables();

            UmbracoContext context = Scaffold.EnsureContext();
            
            IConfigService configService = new ConfigService();

            _contentService = ApplicationContext.Current.Services.ContentService;
            _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

        }

        [Fact]
        public void Can_Convert_To_IPublishedContent()
        {
            Scaffold.ContentType(_contentTypeService);
            IContent node = Scaffold.Node(_contentService);

            IPublishedContent content = node.ToPublishedContent();
            Assert.NotNull(content);
        }
    }
}
