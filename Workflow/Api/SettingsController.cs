using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Caching;
using System.Web.Http;
using log4net;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/settings")]
    public class SettingsController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly PocoRepository Pr = new PocoRepository();

        /// <summary>
        /// Get an object with info about the installed version and latest release from GitHub
        /// </summary>
        /// <returns></returns>
        [Route("version")]
        public IHttpActionResult GetVersion()
        {
            try
            {
                MemoryCache cache = MemoryCache.Default;
                if (cache[MagicStrings.VersionKey] != null)
                {
                    return Json((PackageVersion)cache.Get(MagicStrings.VersionKey), ViewHelpers.CamelCase);
                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;

                var client = new WebClient();
                client.Headers.Add("user-agent", MagicStrings.Name);

                string response = client.DownloadString(MagicStrings.LatestVersionUrl);
                JObject content = JObject.Parse(response);

                string currentVersion = $"v{version.Major}.{version.Minor}.{version.Build}";
                string latestVersion = content["tag_name"].ToString();

                var packageVersion = new PackageVersion
                {
                    CurrentVersion = currentVersion,
                    LatestVersion = latestVersion,
                    ReleaseDate = DateTime.Parse(content["published_at"].ToString()).ToString("d MMMM yyyy"),
                    ReleaseNotes = content["body"].ToString(),
                    PackageUrl = content["assets"][0]["browser_download_url"].ToString(),
                    PackageName = content["assets"][0]["name"].ToString(),
                    OutOfDate = !string.Equals(currentVersion, latestVersion,
                        StringComparison.InvariantCultureIgnoreCase)
                };
                

                // Store data in the cache    
                cache.Add(MagicStrings.VersionKey, packageVersion,
                    new CacheItemPolicy {AbsoluteExpiration = DateTime.Now.AddHours(6)});

                return Json(packageVersion, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                const string error = "Error getting version information";
                Log.Error(error, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, error));
            }
        }

        /// <summary>
        /// Get the documentation from GitHub
        /// </summary>
        /// <returns></returns>
        [Route("docs")]
        public HttpResponseMessage GetDocs()
        {
            try
            {
                string docs;
                var fromCache = false;

                MemoryCache cache = MemoryCache.Default;
                if (cache[MagicStrings.VersionKey] != null)
                {
                    docs = (string)cache.Get(MagicStrings.DocsKey);
                    fromCache = true;
                }
                else
                {
                    var client = new WebClient();
                    client.Headers.Add("user-agent", MagicStrings.Name);
                    client.Headers.Add("accept", MagicStrings.MdMediaType);

                    docs = client.DownloadString(MagicStrings.DocsUrl);
                }

                var response = new HttpResponseMessage
                {
                    Content = new StringContent(docs)
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

                if (!fromCache)
                {
                    // Store data in the cache    
                    cache.Add(MagicStrings.DocsKey, docs, new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddHours(6) });
                }

                return response;
            }
            catch (Exception ex)
            {
                const string error = "Documentation unavailable";
                Log.Error(error, ex);

                return new HttpResponseMessage
                {
                    Content = new StringContent(error)
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("get")]
        public IHttpActionResult Get()
        {
            try
            {
                return Json(Pr.GetSettings(), ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string error = $"Could not get settings'. {ex.Message}";
                Log.Error(error, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, error));
            }
        }

        /// <summary>
        /// Save the settings object
        /// </summary>
        /// <returns>A confirmation message</returns>
        [HttpPost]
        [Route("save")]
        public IHttpActionResult Save(WorkflowSettingsPoco model)
        {
            try
            {
                DatabaseContext.Database.Update(model);
                return Ok("Settings updated");
            }
            catch (Exception ex)
            {
                const string error = "Could not save settings";
                Log.Error(error, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, error));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("getcontenttypes")]
        public IHttpActionResult GetContentTypes()
        {
            try
            {
                List<IContentType> contentTypes = Services.ContentTypeService.GetAllContentTypes().ToList();
                return Json(contentTypes, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                const string error = "Could not get content types";
                Log.Error(error, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, error));
            }
        }
    }
}
