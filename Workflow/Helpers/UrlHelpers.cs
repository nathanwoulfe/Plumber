using System;
using System.Web;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Helpers
{
    public static class UrlHelpers
    {
        private static readonly ISettingsService SettingsService = new SettingsService();

        /// <summary>
        /// This method gives a fully qualified url and can be used without an HTTPContext 
        /// </summary>
        /// <param name="partialUrl">The partial url to fully qualify eg /images/calendar-icon.png</param>
        /// <returns>The fully qualified web site url</returns>
        public static string GetFullyQualifiedSiteUrl(string partialUrl)
        {
            WorkflowSettingsPoco settings = SettingsService.GetSettings();

            string editUrl = settings.EditUrl;
            HttpRequest request = HttpContext.Current.Request;

            if (string.IsNullOrEmpty(editUrl))
            {
                if (request.ApplicationPath != null)
                    editUrl = request.Url.Scheme + "://" + request.Url.Authority +
                              request.ApplicationPath.TrimEnd('/') + "/";
            }

            if (editUrl == null) return string.Empty;

            bool valid = Uri.TryCreate(editUrl, UriKind.Absolute, out Uri uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            // if result is false, the settings value has no scheme, so prepend from the current request, or fallback to https
            if (!valid)
            {
                editUrl = (request.ApplicationPath != null ? request.Url.Scheme : Uri.UriSchemeHttps) + "://" + editUrl;
            }

            var baseUrl = new Uri(editUrl);
            return (new Uri(baseUrl, partialUrl)).ToString();
        }
        
        /// <summary>
        /// Gets the fully qualified url needed to open the back office content editor pane for the document with the given Id
        /// </summary>
        /// <param name="docId"></param>
        /// <returns></returns>
        public static string GetFullyQualifiedContentEditorUrl(int docId)
        {
            return GetFullyQualifiedEditUrl(string.Format(Constants.ContentEditUrlFormat, docId));
        }

        /// <summary>
        /// This method gives a fully qualified Back Office Edit url
        /// </summary>
        /// <param name="partialUrl">The partial url to fully qualify</param>
        /// <returns>The fully qualified Back Office edit url</returns>
        private static string GetFullyQualifiedEditUrl(string partialUrl)
        {
            WorkflowSettingsPoco settings = SettingsService.GetSettings();
            string editUrl = settings.EditUrl;

            if (string.IsNullOrEmpty(editUrl))
            {
                HttpRequest request = HttpContext.Current.Request;
                if (request.ApplicationPath != null)
                    editUrl = request.Url.Scheme + "://" + request.Url.Authority +
                              request.ApplicationPath.TrimEnd('/') + "/";
            }

            if (editUrl == null) return string.Empty;

            var baseUrl = new Uri(editUrl.StartsWith("http") ? editUrl : $"http://{editUrl}");
            return (new Uri(baseUrl, partialUrl)).ToString();
        }
    }
}
