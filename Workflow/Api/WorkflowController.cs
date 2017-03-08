using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;
using Workflow.Models;

namespace Workflow.Api
{
    public class WorkflowController : UmbracoAuthorizedApiController
    {
        private static Database db = ApplicationContext.Current.DatabaseContext.Database;
        private static PocoRepository _pr = new PocoRepository();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetSettings()
        {
            try
            {
                return Json(_pr.GetSettings(), ViewHelpers.CamelCase);
            }
            catch (Exception ex) {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));
            }
        }

        /// <summary>
        /// Save the settings object
        /// </summary>
        /// <returns>A confirmation message</returns>
        [System.Web.Http.HttpPost]
        public IHttpActionResult SaveSettings(WorkflowSettingsPoco model)
        {
            try
            {
                db.Update(model);
                return Ok("Settings updated");
            } 
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));
            }
        }

        /// <summary>
        /// Check if the current node is already in a workflow process
        /// </summary>
        /// <param name="id">The node to check</param>
        /// <returns>A bool indicating the workflow status (true -> workflow active)</returns>
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetStatus(int nodeId)
        {
            try {
                var instances = _pr.InstancesByNodeAndStatus(nodeId, new List<int> { (int)WorkflowStatus.PendingApproval });
                return Ok(instances.Any() ? true : false);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));
            }
        }        
    }
}
