using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Web.WebApi;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Helpers;
using Workflow.Services;
using Workflow.Services.Interfaces;

using TaskStatus = Workflow.Models.TaskStatus;

using UmbConstants = Umbraco.Core.Constants;
using Constants = Workflow.Helpers.Constants;

namespace Workflow.Api
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/tasks")]
    public class TasksController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ITasksService _tasksService;
        private readonly IInstancesService _instancesService;
        private readonly IGroupService _groupService;

        public TasksController() : this(new TasksService(), new InstancesService(), new GroupService())
        {

        }

        public TasksController(ITasksService tasksService, IInstancesService instancesService, IGroupService groupService)
        {
            _tasksService = tasksService;
            _instancesService = instancesService;
            _groupService = groupService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get/{id:int}")]
        public IHttpActionResult GetTask(int id)
        {
            try
            {
                return Json(new
                {
                    task = _tasksService.GetTask(id)
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"Error getting task {id}";
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }

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
                List<WorkflowTaskViewModel> workflowItems = _tasksService.GetPendingTasks(
                    new List<int> {(int) TaskStatus.PendingApproval, (int) TaskStatus.Rejected}, count, page);

                int taskCount = _tasksService.CountPendingTasks();

                return Json(new
                {
                    items = workflowItems,
                    totalPages = (int)Math.Ceiling((double)taskCount / count),
                    page,
                    count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                const string msg = "Error getting pending tasks";
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
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
                List<WorkflowTaskPoco> taskInstances = 
                    _tasksService.GetAllTasksForDateRange(DateTime.Now.AddDays(days * -1));

                return Json(new
                {
                    items = taskInstances
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                const string msg = "Error getting tasks for date range";
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }

        /// <summary>
        /// Returns all tasks
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("filteredRange/{days:int}/{filter?}/{count:int?}/{page:int?}")]
        public IHttpActionResult GetFilteredPagedTasksForDateRange(int days, string filter = "", int? count = null, int? page = null)
        {
            try
            {
                List<WorkflowTaskViewModel> taskInstances =
                    _tasksService.GetFilteredPagedTasksForDateRange(DateTime.Now.AddDays(days * -1), count, page, filter);

                return Json(new
                {
                    items = taskInstances,
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
                // todo -> only fetch the require page, not all
                List<WorkflowTaskPoco> taskInstances = _tasksService.GetTasksByNodeId(id);
                // set sorted to false as the instances are ordered by create date -> sorting will order the paged items by workflow step
                List<WorkflowTaskViewModel> workflowItems = _tasksService.ConvertToWorkflowTaskList(taskInstances.Skip((page - 1) * count).Take(count).ToList(), false);

                return Json(new
                {
                    items = workflowItems,
                    totalPages = (int)Math.Ceiling((double)taskInstances.Count / count),
                    page,
                    count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"Error getting tasks for node {id}";
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
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
            // id will be 0 when creating a new page - id is assigned after save
            if (id == 0)
            {
                return Json(new
                {
                    settings = false,
                    noFlow = false
                }, ViewHelpers.CamelCase);
            }

            try
            {
                WorkflowTaskPoco currentTask = _tasksService.GetTasksByNodeId(id).FirstOrDefault();

                return Json(new
                {
                    items = currentTask != null && currentTask.TaskStatus.In(TaskStatus.PendingApproval, TaskStatus.Rejected) ? 
                        _tasksService.ConvertToWorkflowTaskList(new List<WorkflowTaskPoco> { currentTask }) : 
                        new List<WorkflowTaskViewModel>()
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = Constants.ErrorGettingPendingTasksForNode.Replace("{id}", id.ToString());
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
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
        public async Task<IHttpActionResult> GetFlowsForUser(int userId, int type, int count, int page)
        {
            try
            {
                List<WorkflowTaskPoco> taskInstances = (type == 0
                    ? _tasksService.GetAllPendingTasks(new List<int>
                            {(int) TaskStatus.PendingApproval, (int) TaskStatus.Rejected })
                    : _tasksService.GetTaskSubmissionsForUser(userId, new List<int>
                        {(int) TaskStatus.PendingApproval, (int) TaskStatus.Rejected}))
                    .Where(x => x.WorkflowInstance.Active) 
                    .ToList();
                            

                if (type == 0)
                {
                    foreach (WorkflowTaskPoco taskInstance in taskInstances)
                    {
                        taskInstance.UserGroup = await _groupService.GetPopulatedUserGroupAsync(taskInstance.UserGroup.GroupId);
                    }

                    taskInstances = taskInstances.Where(x => 
                        x.UserGroup.IsMember(userId) ||
                        x.Status == (int) TaskStatus.Rejected && x.WorkflowInstance.AuthorUserId == userId).ToList();
                }

                taskInstances = taskInstances.Where(x => x.WorkflowInstance.Node != null && !x.WorkflowInstance.Node.Path.Contains(UmbConstants.System.RecycleBinContentString)).ToList();
                List<WorkflowTaskViewModel> workflowItems = _tasksService.ConvertToWorkflowTaskList(taskInstances.Skip((page - 1) * count).Take(count).ToList(), false);

                return Json(new
                {
                    items = workflowItems,
                    totalPages = (int)Math.Ceiling((double)taskInstances.Count / count),
                    page,
                    count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"Error trying to build user workflow tasks list for user {userId}";
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
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
                List<WorkflowTaskViewModel> workflowItems = _tasksService.GetAllGroupTasks(groupId, count, page);
                int groupTaskCount = _tasksService.CountGroupTasks(groupId);

                return Json(new
                {
                    items = workflowItems,
                    totalPages = (int)Math.Ceiling((double)groupTaskCount / count),
                    page,
                    count
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"Error getting all tasks for group {groupId}";
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }

        /// <summary>
        /// For a given guid, returns a set of workflow tasks, regardless of task status
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("tasksbyguid/{guid:Guid}")]
        public IHttpActionResult GetTasksByInstanceGuid(Guid guid)
        {
            try
            {
                List<WorkflowTaskPoco> taskInstances = _tasksService.GetTasksWithGroupByInstanceGuid(guid);
                WorkflowInstancePoco instance = _instancesService.GetByGuid(guid);

                return Json(new
                {
                    items = _tasksService.ConvertToWorkflowTaskList(taskInstances, instance: instance),
                    currentStep = taskInstances.Count(x => x.TaskStatus.In(TaskStatus.Approved, TaskStatus.NotRequired)) + 1, // value is for display, so zero-index isn't friendly
                    totalSteps = instance.TotalSteps
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"Error getting tasks by instance guid {guid}";
                Log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }
    }
}