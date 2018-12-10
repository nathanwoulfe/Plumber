/* https://gist.githubusercontent.com/jbreuer/dde3605035179c34b7287850c45cb8c9/raw/570cbaa30365653dbcf4142e988eba4fc692ecad/ContentExtensions.cs */
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Workflow.Models.Content;

namespace Workflow.Extensions
{
    internal static class ContentExtensions
    {
        /// <summary>
        /// Treat unpublished content as IPublishedContent, to allow full property access
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isPreview"></param>
        /// <returns></returns>
        public static IPublishedContent ToPublishedContent(this IContent content, bool isPreview = false)
        {
            return new PublishedContent(content, isPreview);
        }
    }

    internal static class ContentBaseExtensions
    {
        /// <summary>
        /// Gets the url segment providers.
        /// </summary>
        /// <remarks>This is so that unit tests that do not initialize the resolver do not
        /// fail and fall back to defaults. When running the whole Umbraco, CoreBootManager
        /// does initialise the resolver.</remarks>
        private static IEnumerable<IUrlSegmentProvider> UrlSegmentProviders
        {
            get
            {
                return UrlSegmentProviderResolver.HasCurrent
                           ? UrlSegmentProviderResolver.Current.Providers
                           : new IUrlSegmentProvider[] { new DefaultUrlSegmentProvider() };
            }
        }

        /// <summary>
        /// Gets the default url segment for a specified content.
        /// </summary>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <returns>
        /// The url segment.
        /// </returns>
        public static string GetUrlSegment(this IContentBase content)
        {
            var url = UrlSegmentProviders.Select(p => p.GetUrlSegment(content)).First(u => u != null);
            url = url ?? new DefaultUrlSegmentProvider().GetUrlSegment(content); // be safe
            return url;
        }
    }
}
