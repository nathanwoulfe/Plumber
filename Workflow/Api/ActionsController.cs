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
using Workflow.Models;
using Workflow.Extensions;

namespace Workflow.Api
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/actions")]
    public class ActionsController : UmbracoAuthorizedApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static PocoRepository _pr = new PocoRepository();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="authorId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("initiate")]
        public IHttpActionResult InitiateWorkflow(InitiateWorkflowModel model)
        {
            WorkflowInstancePoco instance = null;
            WorkflowApprovalProcess process = null;

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

                instance = process.InitiateWorkflow(int.Parse(model.NodeId), Helpers.GetCurrentUser().Id, model.Comment);

                var msg = string.Empty;

                switch (instance._Status)
                {
                    case WorkflowStatus.PendingApproval:
                        msg = "Page submitted for " + (model.Publish ? "publish" : "unpublish") + " approval.";
                        break;
                    case WorkflowStatus.Approved:
                        msg = (model.Publish ? "Publish" : "Unpublish") + " workflow complete.";

                        if (instance.ScheduledDate.HasValue)
                        {
                            msg += " Page scheduled for publishing at " + instance.ScheduledDate.ToString();
                        }

                        break;
                }

                return Json(new
                {
                    message = msg,
                    status = 200
                }, ViewHelpers.CamelCase);

            }
            catch (Exception e)
            {
                return Json(new
                {
                    message = ViewHelpers.ApiException(e),
                    status = 500
                }, ViewHelpers.CamelCase);
            }
        }


        /// <summary>
        /// Processes the workflow task for the given task id
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("approve")]
        public IHttpActionResult ApproveWorkflowTask(TaskData model)
        {
            var taskId = model.TaskId;
            var comment = model.Comment;
            var instance = GetInstance(taskId);

            try
            {
                WorkflowApprovalProcess process = GetProcess(instance.Type);

                instance = process.ActionWorkflow(
                    instance,
                    WorkflowAction.Approve,
                    Helpers.GetCurrentUser().Id,
                    comment
                );

                string msg = string.Empty;

                switch (instance._Status)
                {
                    case WorkflowStatus.PendingApproval:
                        msg = "Approval completed successfully. Page will be " + instance.TypeDescriptionPastTense.ToLower() + " following workflow completion.";
                        break;
                    case WorkflowStatus.Approved:
                        msg = "Workflow approved successfully.";

                        if (instance.ScheduledDate.HasValue)
                        {
                            msg += " Page scheduled for " + instance.TypeDescription + " at " + instance.ScheduledDate.ToString();
                        }
                        else
                        {
                            msg += " Page has been " + instance.TypeDescriptionPastTense.ToLower();
                        }
                        break;
                }

                return Json(new
                {
                    message = msg,
                    status = 200
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = "An error occurred processing the approval: " + ex.Message + ex.StackTrace;
                log.Error(msg + " for workflow " + instance.Id, ex);
                return Json(new
                {
                    message = msg,
                    status = 500
                }, ViewHelpers.CamelCase);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("reject")]
        public IHttpActionResult RejectWorkflowTask(TaskData model)
        {
            var taskId = model.TaskId;
            var comment = model.Comment;
            var _instance = GetInstance(taskId);

            try
            {
                WorkflowApprovalProcess process = GetProcess(_instance.Type);

                _instance = process.ActionWorkflow(
                    _instance,
                    WorkflowAction.Reject,
                    Helpers.GetCurrentUser().Id,
                    comment
                );

                return Json(new
                {
                    message = _instance.TypeDescription + " request has been rejected.",
                    status = 200
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = "An error occurred rejecting the workflow: " + ex.Message + ex.StackTrace;
                log.Error(msg + " for workflow " + _instance.Id, ex);

                return Json(new
                {
                    message = msg,
                    status = 500
                }, ViewHelpers.CamelCase);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId">The workflow task id</param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("cancel")]
        public IHttpActionResult CancelWorkflowTask(TaskData model)
        {
            var taskId = model.TaskId;
            var comment = model.Comment;
            var _instance = GetInstance(taskId);

            try
            {
                WorkflowApprovalProcess process = GetProcess(_instance.Type);

                _instance = process.CancelWorkflow(
                    _instance,
                    Helpers.GetCurrentUser().Id,
                    comment
                );

                return Json(new
                {
                    status = 200,
                    msg = _instance.TypeDescription + " workflow cancelled"
                }, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                string msg = "An error occurred cancelling the workflow: " + ex.Message + ex.StackTrace;
                log.Error(msg + " for workflow " + _instance.Id, ex);
                return Json(new
                {
                    status = 500,
                    msg = msg
                }, ViewHelpers.CamelCase);
            }
        }

        #region Private methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private dynamic GetProcess(int type)
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
        /// <param name="taskId"></param>
        /// <returns></returns>
        private WorkflowInstancePoco GetInstance(int taskId)
        {
            var _instance = _pr.InstanceByTaskId(taskId);
            _instance.SetScheduledDate();

            // TODO -> fix this
            var tasks = _pr.TasksAndGroupByInstanceId(_instance.Guid);

            if (tasks.Any())
            {
                _instance.TaskInstances = tasks;
            }

            return _instance;
        }

        #endregion
    }
}