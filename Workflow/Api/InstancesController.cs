using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Extensions;

namespace Workflow.Api
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/instances")]
    public class InstancesController : UmbracoAuthorizedApiController
    {
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
                var instances = Pr.GetAllInstances().OrderByDescending(x => x.CreatedDate).ToList();
                var workflowInstances = instances.Skip((page - 1) * count).Take(count).ToList().ToWorkflowInstanceList();
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
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
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
                var instances = Pr.GetAllInstancesForDateRange(DateTime.Now.AddDays(days * -1)).ToWorkflowInstanceList();
                return Json(new
                {
                    items = instances,
                    total = instances.Count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
            }
        }
    }
}