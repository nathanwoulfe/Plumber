using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using Umbraco.Web.WebApi;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Helpers;

namespace Workflow.Api
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/tasks")]
    public class TasksController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly PocoRepository Pr = new PocoRepository();

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
                var taskInstances = Pr.GetPendingTasks((int)TaskStatus.PendingApproval, count, page);
                var workflowItems = taskInstances.ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = Pr.CountPendingTasks(),
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
        public IHttpActionResult GetAllTasksForDateRange(int days)
        {
            try
            {
                var taskInstances = Pr.GetAllTasksForDateRange(DateTime.Now.AddDays(days * -1));
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
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("node/{id:int}/{count:int}/{page:int}")]
        public IHttpActionResult GetNodeTasks(int id, int count, int page)
        {
            try
            {
                var taskInstances = Pr.TasksByNode(id);
                var workflowItems = taskInstances.Skip((page - 1) * count).Take(count).ToList().ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = taskInstances.Count,
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
                var taskInstances = Pr.TasksByNode(id).Where(t => t.Status == (int)TaskStatus.PendingApproval).ToList();
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
        [HttpGet]
        [Route("status/{id:int}")]
        public IHttpActionResult GetStatus(int id)
        {
            try
            {
                var instances = Pr.InstancesByNodeAndStatus(id, new List<int> { (int)WorkflowStatus.PendingApproval });
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
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("flows/{userId:int}/{type:int=0}/{count:int}/{page:int}")]
        public IHttpActionResult GetFlowsForUser(int userId, int type, int count, int page)
        {
            try
            {
                var excludeOwn = Utility.GetSettings().FlowType != (int)FlowType.All;
                var taskInstances = type == 0 ? Pr.TasksForUser(userId, (int)TaskStatus.PendingApproval) : Pr.SubmissionsForUser(userId, (int)TaskStatus.PendingApproval);

                if (excludeOwn && type == 0)
                {
                    taskInstances = taskInstances.Where(t => t.WorkflowInstance.AuthorUserId != Utility.GetCurrentUser().Id).ToList();
                }

                var workflowItems = taskInstances.Skip((page - 1) * count).Take(count).ToList().ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = taskInstances.Count,
                    page,
                    count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                const string s = "Error trying to build user workflow tasks list for user ";
                Log.Error(string.Concat(s + Utility.GetUser(userId).Name, ex));
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
                var taskInstances = Pr.GetAllGroupTasks(groupId, count, page);
                var workflowItems = taskInstances.ToWorkflowTaskList();
                return Json(new
                {
                    items = workflowItems,
                    total = Pr.CountGroupTasks(groupId),
                    page,
                    count
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