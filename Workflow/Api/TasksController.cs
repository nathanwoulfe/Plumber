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
        private List<UserGroupPermissionsPoco> perms = new List<UserGroupPermissionsPoco>();

        #region Public methods

        /// <summary>
        /// Returns all tasks currently in workflow processes
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("pending")]
        public IHttpActionResult GetPendingTasks()
        {
            try
            {
                var taskInstances = _pr.GetPendingTasks((int)TaskStatus.PendingApproval);
                var workflowItems = BuildWorkflowItemList(taskInstances);
                return Json(workflowItems, ViewHelpers.CamelCase);
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
        [Route("all")]
        public IHttpActionResult GetAllTasks()
        {
            try
            {
                var taskInstances = _pr.GetAllTasks();
                var workflowItems = BuildWorkflowItemList(taskInstances).OrderByDescending(x => x.RequestedOn);
                return Json(workflowItems, ViewHelpers.CamelCase);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
            }
        }

        /// <summary>
        /// Returns all workflow instances, with their tasks
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("instances")]
        public IHttpActionResult GetAllInstances()
        {
            try
            {
                var instances = _pr.GetAllInstances();
                var workflowInstances = BuildWorkflowInstanceList(instances).OrderByDescending(x => x.RequestedOn);
                return Json(workflowInstances, ViewHelpers.CamelCase);
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
        [Route("node/{id:int}")]
        public IHttpActionResult GetNodeTasks(int id)
        {
            try
            {
                var taskInstances = _pr.TasksByNode(id);
                var workflowItems = BuildWorkflowItemList(taskInstances);
                return Json(workflowItems, ViewHelpers.CamelCase);
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
        [Route("flows/{userId:int}/{type:int=0}")]
        public IHttpActionResult GetFlowsForUser(int userId, int type)
        {
            try
            {
                var excludeOwn = Helpers.GetSettings().FlowType != (int)FlowType.All;
                var taskInstances = type == 0 ? _pr.TasksForUser(userId, (int)TaskStatus.PendingApproval) : _pr.SubmissionsForUser(userId, (int)TaskStatus.PendingApproval);

                if (excludeOwn && type == 0)
                {
                    taskInstances = taskInstances.Where(t => t.WorkflowInstance.AuthorUserId != Helpers.GetCurrentUser().Id).ToList();
                }

                var workflowItems = BuildWorkflowItemList(taskInstances);
                return Json(workflowItems, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                var s = "Error trying to build user workflow tasks list for user ";
                log.Error(string.Concat(s + Helpers.GetUser(userId).Name, ex));
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, s));
            }
        }        

        /// <summary>
        /// Not sure this is even worth keeping - given use of the grid/archetype etc where data is stored as JSON, the comparator is a big piece of work...
        /// Maybe to do later, but not for alpha/beta 
        /// </summary>
        /// <param name="nodeId">Id of the published node</param>
        /// <param name="taskId">Id of the workflow task</param>
        /// <returns>DifferencesResponseItem</returns>
        [HttpPost]
        public IHttpActionResult ShowDifferences(string nodeId, string taskId)
        {
            throw new NotImplementedException(nodeId + " " + taskId);

            //int _nodeId = int.Parse(nodeId);

            //var publishedVersion = Umbraco.TypedContent(nodeId); // most recent published version
            //var revisedVersion = Services.ContentService.GetById(_nodeId); // current version from database

            //var differences = new DifferencesResponseItem
            //{
            //    CurrentVersionPubDate = publishedVersion.UpdateDate.ToString("d MMM yyyy"),
            //    RevisedVersionPubDate = revisedVersion.UpdateDate.ToString("d MMM yyyy"),
            //};

            //var bodyTextComparison = string.Empty;
            //var keywordsComparison = string.Empty;
            //var descriptionComparison = string.Empty;

            //// Do compare and show differences for documents.
            //foreach (Umbraco.Core.Models.Property p in revisedVersion.Properties)
            //{
            //    var alias = p.Alias;
            //    try
            //    {
            //        if (p.Value != null && alias != "workflow")
            //        {
            //            //new property value... 
            //            string thevalue = library.StripHtml(p.Value.ToString());

            //            var cP = publishedVersion.GetProperty(alias);

            //            if (cP != null && cP.Value != null)
            //            {
            //                string cThevalue = library.StripHtml(cP.Value.ToString());
            //                string compared = Diff.Diff2Html(cThevalue, thevalue);
            //                bool hasChanges = !Equals(compared, thevalue);

            //                // only add comparison rows if changes exist or the property is not empty
            //                if (hasChanges)
            //                {
            //                    var row = "<tr><th>" + alias + ":</th><td>" + library.ReplaceLineBreaks(compared) + "</td></tr>";
            //                    if (alias != "bodyText" && alias != "keywords" && alias != "description")
            //                    {
            //                        differences.CompareData += row;
            //                    }
            //                    else if (alias == "bodyText")
            //                    {
            //                        bodyTextComparison = row;
            //                    }
            //                    else if (alias == "keywords")
            //                    {
            //                        keywordsComparison = row;
            //                    }
            //                    else if (alias == "description")
            //                    {
            //                        descriptionComparison = row;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        differences.CompareData += "<tr><th>" + alias + ":</th><td class=\"alert\">Error processing property: " + ex.Message + "</td></tr>";
            //        log.Error("Error Diffing property " + alias + " for document " + nodeId + ": " + ex.Message);
            //    }
            //}

            //// put bodytext, keywords and description first -> most commonly changed fields
            //differences.CompareData = bodyTextComparison + keywordsComparison + descriptionComparison + differences.CompareData;

            //differences.CompareData += "<table><tbody>" + differences.CompareData + "</tbody></table>";

            //return Json(differences, ViewHelpers.CamelCase);
        }

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
                    message = msg
                }, ViewHelpers.CamelCase);
                
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
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

                return Json(new  {
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

                return Json(new {
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

        #endregion

        #region Private methods

        /// <summary>
        /// Helper method for compiling WorkflowItem response object
        /// </summary>
        /// <param name="taskInstances"></param>
        /// <returns></returns>
        private List<WorkflowTask> BuildWorkflowItemList(List<WorkflowTaskInstancePoco> taskInstances, WorkflowInstancePoco instance = null)
        {

            List<WorkflowTask> workflowItems = new List<WorkflowTask>();

            if (taskInstances != null && taskInstances.Count > 0)
            {
                foreach (var taskInstance in taskInstances)
                {
                    WorkflowInstancePoco useThisInstance = taskInstance.WorkflowInstance != null ? taskInstance.WorkflowInstance : instance;

                    GetPermissionsForNode(useThisInstance.Node);

                    var item = new WorkflowTask
                    {
                        Status = taskInstance.StatusName,
                        CssStatus = taskInstance.StatusName.ToLower().Split(' ')[0],
                        Type = useThisInstance.TypeDescription,
                        NodeId = useThisInstance.NodeId,
                        TaskId = useThisInstance.Id,
                        ApprovalGroupId = taskInstance.UserGroup.GroupId,
                        NodeName = useThisInstance.Node.Name,
                        RequestedBy = useThisInstance.AuthorUser.Name,
                        RequestedOn = taskInstance.CreatedDate.ToString(),
                        ApprovalGroup = taskInstance.UserGroup.Name,
                        Comments = taskInstance.Comment != null ? taskInstance.Comment : useThisInstance.AuthorComment != null ? useThisInstance.AuthorComment : string.Empty,
                        ActiveTask = useThisInstance.StatusName,
                        Permissions = perms,
                        CurrentStep = taskInstance.ApprovalStep
                    };

                    workflowItems.Add(item);
                }
            }

            return workflowItems;
        }

        /// <summary>
        /// Helper method for compiling WorkflowItem response object
        /// </summary>
        /// <param name="taskInstances"></param>
        /// <returns></returns>
        private List<WorkflowInstance> BuildWorkflowInstanceList(List<WorkflowInstancePoco> instances)
        {
            List<WorkflowInstance> workflowInstances = new List<WorkflowInstance>();

            if (instances != null && instances.Count > 0)
            {
                foreach (var instance in instances)
                {
                    var n = Helpers.GetNode(instance.NodeId);
                    var model = new WorkflowInstance
                    {
                        Type = instance.TypeDescription,
                        Status = instance.StatusName,
                        CssStatus = instance.StatusName.ToLower().Split(' ')[0],
                        NodeId = instance.NodeId,
                        NodeName = instance.Node.Name,
                        RequestedBy = instance.AuthorUser.Name,
                        RequestedOn = instance.CreatedDate.ToString(),
                        Tasks = BuildWorkflowItemList(instance.TaskInstances.ToList(), instance).OrderByDescending(x => x.CurrentStep).ToList()
                    };
                    
                    workflowInstances.Add(model);
                }
            }

            return workflowInstances;
        }

        /// <summary>
        /// Get the explicit or implied approval flow for a given node
        /// </summary>
        private void GetPermissionsForNode(IPublishedContent node)
        {
            // check the node for set permissions
            perms = _pr.PermissionsForNode(node.Id, 0);

            // return them if they exist, otherwise check the parent
            if (!perms.Any())
            {
                if (node.Level != 1)
                {
                    GetPermissionsForNode(node.Parent);
                }
                else
                {
                    // check for content-type permissions
                    perms = _pr.PermissionsForNode(0, node.ContentType.Id);
                }
            }
        }

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