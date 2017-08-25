using System;
using System.Net;
using System.Web.Http;
using Umbraco.Core;
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
            catch (Exception ex) {
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
    }
}
