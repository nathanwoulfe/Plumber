using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using log4net;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Extensions;
using Workflow.Models;

namespace Workflow.Api
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/instances")]
    public class InstancesController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly PocoRepository Pr = new PocoRepository();

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
                List<WorkflowInstancePoco> instances = Pr.GetAllInstances().OrderByDescending(x => x.CreatedDate).ToList();
                List<WorkflowInstance> workflowInstances = instances.Skip((page - 1) * count).Take(count).ToList().ToWorkflowInstanceList();
                return Json(new
                {
                    items = workflowInstances,
                    total = instances.Count,
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
        /// Returns all tasks
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("range/{days:int}")]
        public IHttpActionResult GetAllInstancesForDateRange(int days)
        {
            try
            {
                List<WorkflowInstance> instances = Pr.GetAllInstancesForDateRange(DateTime.Now.AddDays(days * -1)).ToWorkflowInstanceList();
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