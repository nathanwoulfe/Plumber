using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using log4net;
using Microsoft.AspNet.SignalR;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.WebApi;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Helpers;
using Workflow.Notifications;
using Workflow.Processes;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Api
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/actions")]
    public class ActionsController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IInstancesService _instancesService;
        private readonly ITasksService _tasksService;
        private readonly IHubContext _hubContext;

        private readonly Utility _utility;

        public ActionsController() : this(new InstancesService(), new TasksService())
        {
        }

        public ActionsController(IInstancesService instancesService, ITasksService tasksService)
        {
            _instancesService = instancesService;
            _tasksService = tasksService;

            _utility = new Utility();
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<PlumberHub>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("initiate")]
        public IHttpActionResult InitiateWorkflow(InitiateWorkflowModel model)
        {
            WorkflowProcess process = null;
            try
            {

                if (model.Publish)
                {
                    process = new DocumentPublishProcess();
                }
                else
                {
                    process = new DocumentUnpublishProcess();
                }

                IUser currentUser = _utility.GetCurrentUser();
                WorkflowInstancePoco instance = process.InitiateWorkflow(int.Parse(model.NodeId), currentUser.Id, model.Comment);

                string msg = string.Empty;

                switch (instance.WorkflowStatus)
                {
                    case WorkflowStatus.PendingApproval:
                        msg = $"Page submitted for {(model.Publish ? "publish" : "unpublish")} approval.";
                        break;
                    case WorkflowStatus.Approved:
                        msg = (model.Publish ? "Publish" : "Unpublish") + " workflow complete.";

                        if (instance.ScheduledDate.HasValue)
                        {
                            msg += $" Page scheduled for publishing at {instance.ScheduledDate.Value.ToString("dd MMM YYYY", CultureInfo.CurrentCulture)}";
                        }

                        break;
                }

                Log.Info(msg);

                // broadcast the new task back to the client to update dashboards etc
                // needs to be converted from a poco to remove unused properties and force camelCase
                _hubContext.Clients.All.WorkflowStarted(
                    _tasksService.ConvertToWorkflowTaskList(instance.TaskInstances.ToList(), instance: instance)
                    .FirstOrDefault());

                return Json(new
                {
                    message = msg,
                    status = 200,
                    notifications = NotificationHelpers.MapEventMessagesToNotifications(process.EventMessages)
                }, ViewHelpers.CamelCase);

            }
            catch (UmbracoOperationFailedException e)
            {
                const string msg = "A Publishing failure occurred initiating the workflow";
                Log.Error(msg, e);

                return Json(new {
                        message = msg,
                        status = 200,
                        isUmbracoOperationError = true,
                        notifications = NotificationHelpers.MapEventMessagesToNotifications(process?.EventMessages)
                    }
                , ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                const string msg = "An error occurred initiating the workflow";
                Log.Error(msg, e);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e, msg));
            }
        }


        /// <summary>
        /// Processes the workflow task for the given taskdata
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("approve")]
        public IHttpActionResult ApproveWorkflowTask(TaskData model)
        {
            WorkflowInstancePoco instance = _instancesService.GetPopulatedInstance(model.InstanceGuid);
            WorkflowProcess process = instance.GetProcess();

            try
            {
                IUser currentUser = _utility.GetCurrentUser();

                instance = process.ActionWorkflow(
                    instance,
                    WorkflowAction.Approve,
                    currentUser.Id,
                    model.Comment
                );

                string msg = string.Empty;
                string logMsg = string.Empty;

                string typeDescription = instance.WorkflowType.Description(instance.ScheduledDate);
                string typeDescriptionPast = instance.WorkflowType.DescriptionPastTense(instance.ScheduledDate);

                switch (instance.WorkflowStatus)
                {
                    case WorkflowStatus.PendingApproval:
                        msg = $"Approval completed successfully. Page will be {typeDescriptionPast.ToLower()} following workflow completion.";
                        logMsg = $"Workflow {typeDescription} task on {instance.Node.Name} [{instance.NodeId}] approved by {currentUser.Name}";
                        break;
                    case WorkflowStatus.Approved:
                        msg = "Workflow approved successfully.";
                        logMsg = $"Workflow approved by {currentUser.Name} on {instance.Node.Name} [{instance.NodeId}]";

                        if (instance.ScheduledDate.HasValue)
                        {
                            string scheduled = $" Page scheduled for {typeDescription} at {instance.ScheduledDate.Value.ToString("dd MMM YYYY", CultureInfo.CurrentCulture)}";
                            msg += scheduled;
                            logMsg += scheduled;
                        }
                        else
                        {
                            msg += $" Page has been {typeDescriptionPast.ToLower()}";
                        }
                        break;
                }

                Log.Info(logMsg);

                _hubContext.Clients.All.TaskApproved(
                    _tasksService.ConvertToWorkflowTaskList(instance.TaskInstances.ToList(), instance: instance));

                return Json(new
                {
                    message = msg,
                    status = 200,
                    notifications = NotificationHelpers.MapEventMessagesToNotifications(process.EventMessages)
                }, ViewHelpers.CamelCase);
            }
            catch (UmbracoOperationFailedException e)
            {
                string msg = $"A Publishing failure occurred processing the approval on {instance.Node.Name} [{instance.Node.Id}]";
                Log.Error(msg, e);

                return Json(new
                    {
                        message = msg,
                        status = 200,
                        isUmbracoOperationError = true,
                        notifications = NotificationHelpers.MapEventMessagesToNotifications(process.EventMessages)
                    }
                    , ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"An error occurred processing the approval on {instance.Node.Name} [{instance.Node.Id}]";
                Log.Error(msg, ex);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("reject")]
        public IHttpActionResult RejectWorkflowTask(TaskData model)
        {
            WorkflowInstancePoco instance = _instancesService.GetPopulatedInstance(model.InstanceGuid);

            try
            {
                WorkflowProcess process = instance.GetProcess();
                IUser currentUser = _utility.GetCurrentUser();

                instance = process.ActionWorkflow(
                    instance,
                    WorkflowAction.Reject,
                    currentUser.Id,
                    model.Comment
                );

                string typeDescription = instance.WorkflowType.Description(instance.ScheduledDate);

                Log.Info($"{typeDescription} request for {instance.Node.Name} [{instance.NodeId}] was rejected by {currentUser.Name}");

                _hubContext.Clients.All.TaskRejected(
                    _tasksService.ConvertToWorkflowTaskList(instance.TaskInstances.ToList(), instance: instance));

                return Json(new
                {
                    message = typeDescription + " request has been rejected.",
                    status = 200
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"An error occurred rejecting the workflow on {instance.Node.Name} [{instance.NodeId}]";
                Log.Error(msg, ex);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("cancel")]
        public IHttpActionResult CancelWorkflowTask(TaskData model)
        {
            WorkflowInstancePoco instance = _instancesService.GetPopulatedInstance(model.InstanceGuid);

            try
            {
                WorkflowProcess process = instance.GetProcess();
                IUser currentUser = _utility.GetCurrentUser();

                instance = process.CancelWorkflow(
                    instance,
                    currentUser.Id,
                    model.Comment
                );

                string typeDescription = instance.WorkflowType.Description(instance.ScheduledDate);

                Log.Info($"{typeDescription} request for {instance.Node.Name} [{instance.NodeId}] was cancelled by {currentUser.Name}");

                _hubContext.Clients.All.TaskCancelled(
                    _tasksService.ConvertToWorkflowTaskList(instance.TaskInstances.ToList(), instance: instance)
                        .LastOrDefault());

                return Json(new
                {
                    status = 200,
                    message = typeDescription + " workflow cancelled"
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"An error occurred cancelling the workflow on {instance.Node.Name} [{instance.NodeId}]";
                Log.Error(msg, ex);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }

        /// <summary>
        /// Endpoint for resubmitting a workflow when the previous task was rejected
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("resubmit")]
        public IHttpActionResult ResubmitWorkflowTask(TaskData model)
        {
            WorkflowInstancePoco instance = _instancesService.GetPopulatedInstance(model.InstanceGuid);
            WorkflowProcess process = instance.GetProcess();

            try
            {
                IUser currentUser = _utility.GetCurrentUser();

                instance = process.ResubmitWorkflow(
                    instance,
                    currentUser.Id,
                    model.Comment
                );

                string typeDescription = instance.WorkflowType.Description(instance.ScheduledDate);
                string typeDescriptionPast = instance.WorkflowType.DescriptionPastTense(instance.ScheduledDate);

                Log.Info($"{typeDescription} request for {instance.Node.Name} [{instance.NodeId}] was resubmitted by {currentUser.Name}");

                _hubContext.Clients.All.TaskResubmitted(
                    _tasksService.ConvertToWorkflowTaskList(instance.TaskInstances.ToList(), instance: instance));

                return Json(new
                {
                    message = $"Changes resubmitted successfully. Page will be {typeDescriptionPast.ToLower()} following workflow completion.",
                    status = 200,
                    notifications = NotificationHelpers.MapEventMessagesToNotifications(process.EventMessages)
                }, ViewHelpers.CamelCase);
            }
            catch (UmbracoOperationFailedException e)
            {
                string msg = $"A Publishing failure occurred processing the approval on {instance.Node.Name} [{instance.Node.Id}]";
                Log.Error(msg, e);

                return Json(new
                    {
                        message = msg,
                        status = 200,
                        isUmbracoOperationError = true,
                        notifications = NotificationHelpers.MapEventMessagesToNotifications(process.EventMessages)
                    }
                    , ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"An error occurred processing the approval on {instance.Node.Name} [{instance.NodeId}]";
                Log.Error(msg, ex);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }
    }
}