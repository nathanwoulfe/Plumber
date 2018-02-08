using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/settings")]
    public class SettingsController : UmbracoAuthorizedApiController
    {
        private static readonly Database Db = ApplicationContext.Current.DatabaseContext.Database;
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
                Assembly assembly = Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;

                var client = new WebClient();
                client.Headers.Add("user-agent", MagicStrings.Name);

                var response = client.DownloadString(MagicStrings.LatestVersionUrl);
                var content = JObject.Parse(response);

                var currentVersion = $"v{version.Major}.{version.Minor}.{version.Build}";
                var latestVersion = content["tag_name"].ToString();

                return Json(new PackageVersion
                {
                    CurrentVersion = currentVersion,
                    LatestVersion = latestVersion,
                    ReleaseDate = DateTime.Parse(content["published_at"].ToString()).ToString("d MMMM yyyy"),
                    ReleaseNotes = content["body"].ToString(),
                    PackageUrl = content["assets"][0]["browser_download_url"].ToString(),
                    PackageName = content["assets"][0]["name"].ToString(),
                    OutOfDate = !string.Equals(currentVersion, latestVersion, StringComparison.InvariantCultureIgnoreCase)
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));
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
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));
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
                Db.Update(model);
                return Ok("Settings updated");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));
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
                List<IContentType> contentTypes = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes().ToList();
                return Json(contentTypes, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));

            }
        }
    }
}
