using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Web.Http;
using log4net;
using log4net.Appender;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;
using Logger = log4net.Repository.Hierarchy.Logger;

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
                const string error = "Could not get settings";
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

        private static string[] ReverseLog(string[] log)
        {
            List<string> resp = new List<string>();

            while (log.Length != 0)
            {
                int index = Array.FindLastIndex(log, x => char.IsDigit(x[0]));
                for (int i = index; i < log.Length; i += 1)
                {
                    resp.Add(log[i]);
                }

                log = log.Take(index).ToArray();
            }

            return resp.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("log")]
        public HttpResponseMessage GetLog()
        {
            var response = new HttpResponseMessage();

            try
            {
                // read the contents of the log file
                ILog log = LogManager.GetLogger("Workflow");
                var logger = (Logger)log.Logger;
                var appender = (FileAppender)logger.GetAppender("WorkflowLogAppender");
                string filename = appender.File;
                string logText = System.IO.File.ReadAllText(filename);

                if (!string.IsNullOrEmpty(logText))
                {
                    string[] splitLog = logText.Split(new[] { Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

                    splitLog = ReverseLog(splitLog);

                    var html = "";

                    foreach (string line in splitLog)
                    {
                        // really naive parsing of log string into basic html
                        // we know the structure based on the logger patten, so can split relatively confidently
                        string lineClass = char.IsDigit(line[0]) ? "row" : "detail";
                        string[] splitLine = line.Split(' ');

                        html += $"<span class=\"log-{lineClass} {(lineClass == "row" ? splitLine[3].ToLower() : "")}\">";

                        if (lineClass == "row")
                        {
                            // date/time
                            DateTime date = DateTime.ParseExact($"{splitLine[0]} {splitLine[1]}", "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.CurrentCulture);
                            html += $"<span class=\"log-date\">{date.ToString("MMM dd yyyy, h:mmtt: ", CultureInfo.CurrentCulture)}</span> ";

                            // thread
                            html += $"<span class=\"log-thread\">{splitLine[2]}</span> ";

                            // type
                            html += $"<span class=\"log-type {splitLine[3].ToLower()}\">{splitLine[3]}</span> ";

                            // class
                            html += $"<span class=\"log-class\">{splitLine[4]}</span> {splitLine[5]} ";

                            // class
                            html += $"<span class=\"log-message\">{line.Substring(line.IndexOf("- ") + 2)}</span> ";
                        }
                        else
                        {
                            // class
                            html += $"<span class=\"log-message\">{line}</span> ";
                        }

                        // close it all
                        html += "</span>";

                    }

                    logText = html;
                }
                else
                {
                    logText = "*** Log file is currently empty ***";
                }

                response.Content = new StringContent(logText);

            }
            catch (Exception ex)
            {
                const string error = "Could not get workflow log";
                Log.Error(error, ex);
                response.Content = new StringContent(error);

            }

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;

        }
    }
}
