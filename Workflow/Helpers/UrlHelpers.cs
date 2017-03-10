using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Workflow
{
    public class UrlHelpers
    {
        public const string ContentEditUrlFormat = "/umbraco#/content/content/edit/{0}";
        public const string ContentFrameUrlFormat = "/umbraco/backoffice/UmbracoApi/Content/GetById?id={0}";

        /// <summary>
        /// This method gives a fully qualified url and can be used without an HTTPContext 
        /// </summary>
        /// <param name="partialUrl">The partial url to fully qualify eg /images/calendar-icon.png</param>
        /// <returns>The fully qualified web site url</returns>
        public static string GetFullyQualifiedSiteUrl(string partialUrl)
        {
            var editUrl = Helpers.GetSettings().EditUrl;
            if (string.IsNullOrEmpty(editUrl))
            {
                var request = HttpContext.Current.Request;
                editUrl = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/') + "/";
            }

            Uri baseUrl = new Uri(editUrl);
            return (new Uri(baseUrl, partialUrl)).ToString();
        }

        /// <summary>
        /// This method gives a fully qualified Back Office Edit url
        /// </summary>
        /// <param name="partialUrl">The partial url to fully qualify</param>
        /// <returns>The fully qualified Back Office edit url</returns>
        public static string GetFullyQualifiedEditUrl(string partialUrl)
        {
            var editUrl = Helpers.GetSettings().EditUrl;
            if (string.IsNullOrEmpty(editUrl))
            {
                var request = HttpContext.Current.Request;
                editUrl = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/') + "/";
            }

            Uri baseUrl = new Uri(editUrl);
            return (new Uri(baseUrl, partialUrl)).ToString();
        }

        // Gets the fully qualified url needed to open the back office content editor pane for the document with the given Id
        public static string GetFullyQualifiedContentEditorUrl(int docId)
        {
            return GetFullyQualifiedEditUrl(string.Format(ContentEditUrlFormat, docId));
        }
    }
}
