using System;
using System.Collections.Generic;
using System.Linq;
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
                List<WorkflowInstance> workflowInstances = _instancesService.Get(page, count);

                return Json(new
                {
                    items = workflowInstances,
                    totalPages = (int)Math.Ceiling(_instancesService.CountAll() / count),
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
        /// Returns all workflow instances, with their tasks for the given node id
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("{nodeId:int}/{count:int}/{page:int}")]
        public IHttpActionResult GetAllInstancesByNodeId(int nodeId, int count, int page)
        {
            try
            {
                List<WorkflowInstance> workflowInstances = _instancesService.GetByNodeId(nodeId, page, count);

                return Json(new
                {
                    items = workflowInstances,
                    totalPages = (int)Math.Ceiling(_instancesService.CountAll() / count),
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
        public IHttpActionResult GetAllInstancesForRange(int days)
        {
            try
            {
                List<WorkflowInstance> instances = _instancesService.GetAllInstancesForDateRange(DateTime.Now.AddDays(days * -1));

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

        /// <summary>
        /// Returns all tasks
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("filteredRange/{days:int}/{filter?}/{count:int?}/{page:int?}")]
        public IHttpActionResult GetFilteredPagedInstancesForDateRange(int days, string filter = "", int? count = null, int? page = null)
        {
            try
            {
                List<WorkflowInstance> instances =
                    _instancesService.GetFilteredPagedInstancesForDateRange(DateTime.Now.AddDays(days * -1), count, page, filter);

                return Json(new
                {
                    items = instances,
                    count,
                    page,
                    filter
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                const string msg = "Error getting tasks for date range";
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }

    }
}