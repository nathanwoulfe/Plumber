using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using log4net;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using Workflow.Models;
using Workflow.Helpers;
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

        private readonly Utility _utility;

        public ActionsController()
        {
            _instancesService = new InstancesService();
            _tasksService = new TasksService();

            _utility = new Utility();
        }

        public ActionsController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
            _instancesService = new InstancesService();
            _tasksService = new TasksService();

            _utility = new Utility();
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
            try
            {
                WorkflowProcess process;

                if (model.Publish)
                {
                    process = new DocumentPublishProcess();
                }
                else
                {
                    process = new DocumentUnpublishProcess();
                }

                WorkflowInstancePoco instance = process.InitiateWorkflow(int.Parse(model.NodeId), _utility.GetCurrentUser().Id, model.Comment);

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

                return Json(new
                {
                    message = msg,
                    status = 200
                }, ViewHelpers.CamelCase);

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
            WorkflowInstancePoco instance = GetInstance(model.InstanceGuid);

            try
            {
                WorkflowProcess process = GetProcess(instance.Type);
                IUser currentUser = _utility.GetCurrentUser();

                instance = process.ActionWorkflow(
                    instance,
                    WorkflowAction.Approve,
                    currentUser.Id,
                    model.Comment
                );

                string msg = string.Empty;
                string logMsg = string.Empty;
                   
                switch (instance.WorkflowStatus)
                {
                    case WorkflowStatus.PendingApproval:
                        msg = $"Approval completed successfully. Page will be {instance.TypeDescriptionPastTense.ToLower()} following workflow completion.";
                        logMsg = $"Workflow {instance.TypeDescription} task on {instance.Node.Name} [{instance.NodeId}] approved by {currentUser.Name}";
                        break;
                    case WorkflowStatus.Approved:
                        msg = "Workflow approved successfully.";
                        logMsg = $"Workflow approved by {currentUser.Name} on {instance.Node.Name} [{instance.NodeId}]";

                        if (instance.ScheduledDate.HasValue)
                        {
                            string scheduled = $" Page scheduled for {instance.TypeDescription} at {instance.ScheduledDate.Value.ToString("dd MMM YYYY", CultureInfo.CurrentCulture)}";
                            msg += scheduled;
                            logMsg += scheduled;
                        }
                        else
                        {
                            msg += $" Page has been {instance.TypeDescriptionPastTense.ToLower()}";
                        }
                        break;
                }

                Log.Info(logMsg);

                return Json(new
                {
                    message = msg,
                    status = 200
                }, ViewHelpers.CamelCase);
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
            WorkflowInstancePoco instance = GetInstance(model.InstanceGuid);

            try
            {
                WorkflowProcess process = GetProcess(instance.Type);
                IUser currentUser = _utility.GetCurrentUser();

                instance = process.ActionWorkflow(
                    instance,
                    WorkflowAction.Reject,
                    currentUser.Id,
                    model.Comment
                );

                Log.Info($"{instance.TypeDescription} request for {instance.Node.Name} [{instance.NodeId}] was rejected by {currentUser.Name}");

                return Json(new
                {
                    message = instance.TypeDescription + " request has been rejected.",
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
            WorkflowInstancePoco instance = GetInstance(model.InstanceGuid);

            try
            {
                WorkflowProcess process = GetProcess(instance.Type);
                IUser currentUser = _utility.GetCurrentUser();

                instance = process.CancelWorkflow(
                    instance,
                    currentUser.Id,
                    model.Comment
                );

                Log.Info($"{instance.TypeDescription} request for {instance.Node.Name} [{instance.NodeId}] was cancelled by {currentUser.Name}");

                return Json(new
                {
                    status = 200,
                    message = instance.TypeDescription + " workflow cancelled"
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
            WorkflowInstancePoco instance = GetInstance(model.InstanceGuid);

            try
            {
                WorkflowProcess process = GetProcess(instance.Type);
                IUser currentUser = _utility.GetCurrentUser();

                instance = process.ResubmitWorkflow(
                    instance,
                    currentUser.Id,
                    model.Comment
                );

                Log.Info($"{instance.TypeDescription} request for {instance.Node.Name} [{instance.NodeId}] was resubmitted by {currentUser.Name}");

                return Json(new
                {
                    message = $"Changes resubmitted successfully. Page will be {instance.TypeDescriptionPastTense.ToLower()} following workflow completion.",
                    status = 200
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = $"An error occurred processing the approval on {instance.Node.Name} [{instance.NodeId}]";
                Log.Error(msg, ex);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }

        #region Private methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static dynamic GetProcess(int type)
        {
            if ((WorkflowType)type == WorkflowType.Publish)
            {
                return new DocumentPublishProcess();
            }
            return new DocumentUnpublishProcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceGuid"></param>
        /// <returns></returns>
        private WorkflowInstancePoco GetInstance(Guid instanceGuid)
        {
            WorkflowInstancePoco instance = _instancesService.GetByGuid(instanceGuid);
            instance.SetScheduledDate();

            // TODO -> fix this
            List<WorkflowTaskInstancePoco> tasks = _tasksService.GetTasksWithGroupByInstanceGuid(instance.Guid);

            if (tasks.Any())
            {
                // ordering by approval step is most logical
                instance.TaskInstances = tasks.OrderBy(t => t.ApprovalStep).ToList();
            }

            return instance;
        }

        #endregion
    }
}