using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Web.Http;
using log4net;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Api
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/instances")]
    public class InstancesController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IInstancesService _instancesService;

        public InstancesController()
        {
            _instancesService = new InstancesService();
        }
        
        /// <summary>
        /// Returns all workflow instances, with their tasks
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("{count:int}/{page:int}")]
        public IHttpActionResult GetAllInstances(int count, int page)
        {
            try
            {
                List<WorkflowInstance> workflowInstances = _instancesService.Get(page, count, null);

                return Json(new
                {
                    items = workflowInstances,
                    total = _instancesService.CountPending(),
                    page,
                    count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                const string error = "Error getting workflow instances";
                Log.Error(error, e);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e, error));
            }
        }

        /// <summary>
        /// Returns all instances
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("range/{days:int}")]
        public IHttpActionResult GetAllInstancesForDateRange(int days)
        {
            try
            {
                List<WorkflowInstance> instances = _instancesService.Get(null, null, DateTime.Now.AddDays(days * -1));

                return Json(new
                {
                    items = instances,
                    total = instances.Count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                const string error = "Error getting instances for date range";
                Log.Error(error, e);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e, error));
            }
        }
    }
}