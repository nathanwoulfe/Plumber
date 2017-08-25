using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using umbraco;
using umbraco.cms.businesslogic.utilities;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;
using Workflow.Extensions;
using Workflow.Models;

namespace Workflow.Api
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/tasks")]
    public class TasksController : UmbracoAuthorizedApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static PocoRepository _pr = new PocoRepository();

        #region Public methods

        /// <summary>
        /// Returns all tasks currently in workflow processes
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("pending/{count:int}/{page:int}")]
        public IHttpActionResult GetPendingTasks(int count, int page)
        {
            try
            {
                var taskInstances = _pr.GetPendingTasks((int)TaskStatus.PendingApproval, count, page);
                var workflowItems = taskInstances.ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = _pr.CountPendingTasks(),
                    page = page,
                    count = count
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
        public IHttpActionResult GetAllTasksForDateRange(int days)
        {
            try
            {
                var taskInstances = _pr.GetAllTasksForDateRange(DateTime.Now.AddDays(days * -1));
                return Json(new
                {
                    items = taskInstances,
                    total = taskInstances.Count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
            }
        }        

        /// <summary>
        /// Return workflow tasks for the given node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("node/{id:int}/{count:int}/{page:int}")]
        public IHttpActionResult GetNodeTasks(int id, int count, int page)
        {
            try
            {
                var taskInstances = _pr.TasksByNode(id);
                var workflowItems = taskInstances.Skip((page - 1) * count).Take(count).ToList().ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = taskInstances.Count,
                    page = page,
                    count = count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
            }
        }

        /// <summary>
        /// Return workflow tasks for the given node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("node/pending/{id:int}")]
        public IHttpActionResult GetNodePendingTasks(int id)
        {
            try
            {
                var taskInstances = _pr.TasksByNode(id).Where(t => t.Status == (int)TaskStatus.PendingApproval).ToList();
                var workflowItems = taskInstances.ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = taskInstances.Count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
            }
        }

        /// <summary>
        /// Check if the current node is already in a workflow process
        /// </summary>
        /// <param name="id">The node to check</param>
        /// <returns>A bool indicating the workflow status (true -> workflow active)</returns>
        [System.Web.Http.HttpGet]
        [Route("status/{id:int}")]
        public IHttpActionResult GetStatus(int id)
        {
            try
            {
                var instances = _pr.InstancesByNodeAndStatus(id, new List<int> { (int)WorkflowStatus.PendingApproval });
                return Ok(instances.Any());
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));
            }
        }

        /// <summary>
        /// Gets all tasks requiring actioning by the current user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type">0 - tasks, 1 - submissions</param>
        /// <returns></returns>
        [HttpGet]
        [Route("flows/{userId:int}/{type:int=0}/{count:int}/{page:int}")]
        public IHttpActionResult GetFlowsForUser(int userId, int type, int count, int page)
        {
            try
            {
                var excludeOwn = Utility.GetSettings().FlowType != (int)FlowType.All;
                var taskInstances = type == 0 ? _pr.TasksForUser(userId, (int)TaskStatus.PendingApproval) : _pr.SubmissionsForUser(userId, (int)TaskStatus.PendingApproval);

                if (excludeOwn && type == 0)
                {
                    taskInstances = taskInstances.Where(t => t.WorkflowInstance.AuthorUserId != Utility.GetCurrentUser().Id).ToList();
                }

                var workflowItems = taskInstances.Skip((page - 1) * count).Take(count).ToList().ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = taskInstances.Count,
                    page = page,
                    count = count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                var s = "Error trying to build user workflow tasks list for user ";
                log.Error(string.Concat(s + Utility.GetUser(userId).Name, ex));
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, s));
            }
        }

        /// <summary>
        /// Returns all tasks
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("group/{groupId:int}/{count:int}/{page:int}")]
        public IHttpActionResult GetAllTasksForGroup(int groupId, int count = 10, int page = 1)
        {
            try
            {
                var taskInstances = _pr.GetAllGroupTasks(groupId, count, page);
                var workflowItems = taskInstances.ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = _pr.CountGroupTasks(groupId),
                    page = page,
                    count = count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
            }
        }

        #endregion
    }
}