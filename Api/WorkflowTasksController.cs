using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using umbraco;
using umbraco.cms.businesslogic.utilities;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;
using Workflow.Models;

namespace Workflow.Dashboard
{
    /// <summary>
    /// WebAPI methods for generating the user workflow dashboard
    /// </summary>
    public class WorkflowTasksController : UmbracoAuthorizedApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Database db = ApplicationContext.Current.DatabaseContext.Database;

        /// <summary>
        /// Returns all tasks currently in workflow processes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<WorkflowItem> GetActiveTasks()
        {
            var taskInstances = PocoRepository.TasksByStatus((int)TaskStatus.PendingApproval);            
            var workflowItems = BuildWorkflowItemList(taskInstances, -1, false);
            return workflowItems.AsEnumerable();
        }

        /// <summary>
        /// Gets all tasks requiring actioning by the current user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<WorkflowItem> GetApprovalsForUser(string userId)
        {
            var _userId = int.Parse(userId);
            var workflowItems = new List<WorkflowItem>();

            try
            {
                var userUserGroups = PocoRepository.GroupsForUserById(_userId);
                var taskInstances = PocoRepository.TasksWithGroup().ApprovalTasksForUserGroups(userUserGroups).ToList();

                workflowItems = BuildWorkflowItemList(taskInstances, _userId);
            }
            catch (Exception ex)
            {
                log.Error("Error trying to build user workflow tasks list for user " + Services.UserService.GetUserById(_userId).Name, ex);
            }

            return workflowItems.AsEnumerable();
        }


        /// <summary>
        /// Gets all current workflow processes initiated by the requesting user
        /// </summary>
        /// <returns>IEnumerable<WorkflowItem></returns>
        [HttpPost]
        public IEnumerable<WorkflowItem> GetSubmissionsForUser(string userId)
        {
            int _userId = int.Parse(userId);
            List<WorkflowItem> workflowItems = new List<WorkflowItem>();

            try
            {
                var taskInstances = PocoRepository.TasksByUserAndStatus(_userId, (int)TaskStatus.PendingApproval);
                workflowItems = BuildWorkflowItemList(taskInstances, _userId);

            }
            catch (Exception ex)
            {
                log.Error("Error trying to build user workflow tasks list for user " + Services.UserService.GetUserById(_userId).Name, ex);
            }

            return workflowItems.AsEnumerable();
        }


        /// <summary>
        /// Finds and highlights differences between the last published version of a document, and the currently workflowed version
        /// </summary>
        /// <param name="nodeId">Id of the published node</param>
        /// <param name="taskId">Id of the workflow task</param>
        /// <returns>DifferencesResponseItem</returns>
        [HttpPost]
        public HttpResponseMessage ShowDifferences(string nodeId, string taskId)
        {
            int _nodeId = int.Parse(nodeId);
            int _taskId = int.Parse(taskId);

            var _instance = PocoRepository.InstanceByTaskId(_taskId);
            var publishedVersion = Umbraco.TypedContent(nodeId); // most recent published version
            var revisedVersion = Services.ContentService.GetById(_nodeId); // current version from database

            var differences = new DifferencesResponseItem
            {
                CurrentVersionPubDate = publishedVersion.UpdateDate.ToString("d MMM yyyy"),
                RevisedVersionPubDate = revisedVersion.UpdateDate.ToString("d MMM yyyy"),
               // CompareMessage = ui.Text("rollback", "approvaldiffHelp")
            };

            //differences.CompareData += "<table><tbody>";
            var bodyTextComparison = string.Empty;
            var keywordsComparison = string.Empty;
            var descriptionComparison = string.Empty;

            // Do compare and show differences for documents.
            foreach (Umbraco.Core.Models.Property p in revisedVersion.Properties)
            {
                var alias = p.Alias;
                try
                {
                    if (p.Value != null && alias != "workflow")
                    {
                        //new property value... 
                        string thevalue = library.StripHtml(p.Value.ToString());

                        var cP = publishedVersion.GetProperty(alias);

                        if (cP != null && cP.Value != null)
                        {
                            string cThevalue = library.StripHtml(cP.Value.ToString());
                            string compared = Diff.Diff2Html(cThevalue, thevalue);
                            bool hasChanges = !Equals(compared, thevalue);

                            // only add comparison rows if changes exist or the property is not empty
                            if (hasChanges)
                            {
                                var row = "<tr><th>" + alias + ":</th><td>" + library.ReplaceLineBreaks(compared) + "</td></tr>";
                                if (alias != "bodyText" && alias != "keywords" && alias != "description")
                                {
                                    differences.CompareData += row;
                                }
                                else if (alias == "bodyText")
                                {
                                    bodyTextComparison = row;
                                }
                                else if (alias == "keywords")
                                {
                                    keywordsComparison = row;
                                }
                                else if (alias == "description")
                                {
                                    descriptionComparison = row;
                                }
                            }
                        }
                        //else
                        //{
                        //    //If no current version of the value... display with no diff.
                        //    differences.CompareData += "<tr><th>" + alias + ":</th><td>" + thevalue + "</td></tr>";
                        //}
                    }
                }
                catch (Exception ex)
                {
                    differences.CompareData += "<tr><th>" + alias + ":</th><td class=\"alert\">Error processing property: " + ex.Message + "</td></tr>";
                    log.Error("Error Diffing property " + alias + " for document " + nodeId + ": " + ex.Message);
                }
            }

            // put bodytext, keywords and description first -> most commonly changed fields
            differences.CompareData = bodyTextComparison + keywordsComparison + descriptionComparison + differences.CompareData;

            differences.CompareData += "<table><tbody>" + differences.CompareData + "</tbody></table>";

            return Request.CreateResponse(HttpStatusCode.OK, differences);
        }


        /// <summary>
        /// Processes the workflow task for the given task id
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage ApproveWorkflowTask(string taskId, string comment = "")
        {
            var _instance = GetInstance(taskId);

            try
            {
                TwoStepApprovalProcess process = GetProcess(_instance, db);

                _instance = process.ActionWorkflow(
                    _instance,
                    WorkflowAction.Approve,
                    Services.UserService.GetByUsername(HttpContext.Current.User.Identity.Name).Id,
                    comment
                );

                string msg = string.Empty;

                switch (_instance._Status)
                {
                    case WorkflowStatus.PendingFinalApproval:
                        msg = "Coordinator approval completed successfully. Page will be " + _instance.TypeDescriptionPastTense.ToLower() + " pending final approval.";
                        break;
                    case WorkflowStatus.Completed:
                        msg = "Workflow approved successfully and page " + _instance.TypeDescriptionPastTense.ToLower();
                        break;
                }

                var respMessage = new WorkflowResponseItem
                {
                    Message = msg,
                    Type = _instance._Type
                };

                return Request.CreateResponse(HttpStatusCode.OK, respMessage);
            }
            catch (Exception ex)
            {
                string msg = "An error occurred processing the approval: " + ex.Message + ex.StackTrace;
                log.Error(msg + " for workflow " + _instance.Id, ex);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError(msg));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage RejectWorkflowTask(string taskId, string comment = "")
        {
            var _instance = GetInstance(taskId);

            try
            {
                TwoStepApprovalProcess process = GetProcess(_instance, db);

                _instance = process.ActionWorkflow(
                    _instance,
                    WorkflowAction.Reject,
                    Services.UserService.GetByUsername(HttpContext.Current.User.Identity.Name).Id,
                    comment
                );

                return Request.CreateResponse(HttpStatusCode.OK, new WorkflowResponseItem
                {
                    Message = _instance.TypeDescription + " request has been rejected.",
                    Type = _instance._Type
                });
            }
            catch (Exception ex)
            {
                string msg = "An error occurred rejecting the workflow: " + ex.Message + ex.StackTrace;
                log.Error(msg + " for workflow " + _instance.Id, ex);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError(msg));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId">The workflow task id</param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage CancelWorkflowTask(string taskId, string comment = null)
        {
            var _instance = GetInstance(taskId);

            try
            {
                TwoStepApprovalProcess process = GetProcess(_instance, db);

                _instance = process.CancelWorkflow(
                    _instance,
                    Services.UserService.GetByUsername(HttpContext.Current.User.Identity.Name).Id,
                    comment
                );

                return Request.CreateResponse(HttpStatusCode.OK, new WorkflowResponseItem
                {
                    Message = _instance.TypeDescription + " workflow cancelled",
                    Type = _instance._Type
                });
            }
            catch (Exception ex)
            {
                string msg = "An error occurred cancelling the workflow: " + ex.Message + ex.StackTrace;
                log.Error(msg + " for workflow " + _instance.Id, ex);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError(msg));
            }
        }


        /// <summary>
        /// Helper method for compiling WorkflowItem response object
        /// </summary>
        /// <param name="taskInstances"></param>
        /// <returns></returns>
        public List<WorkflowItem> BuildWorkflowItemList(List<WorkflowTaskInstancePoco> taskInstances, int _userId = -1, bool includeActionLinks = true)
        {
            List<WorkflowItem> workflowItems = new List<WorkflowItem>();

            try
            {
                if (taskInstances != null && taskInstances.Count > 0)
                {
                    foreach (var taskInstance in taskInstances)
                    {
                        // TODO -> fix this
                        var tasks = PocoRepository.TasksByInstanceId(taskInstance.WorkflowInstanceGuid);
                        if (tasks.Any())
                        {
                            taskInstance.WorkflowInstance.TaskInstances = tasks;
                        }

                        var users = PocoRepository.UsersByGroupId(taskInstance.GroupId);
                        if (users.Any())
                        {
                            taskInstance.UserGroup.Users = users;
                        }

                        var item = new WorkflowItem
                        {
                            Type = taskInstance.WorkflowInstance.TypeDescription,
                            NodeId = taskInstance.WorkflowInstance.NodeId,
                            TaskId = taskInstance.WorkflowInstance.Id,
                            RequestedBy = taskInstance.WorkflowInstance.AuthorUser.Name,
                            RequestedOn = taskInstance.CreatedDate.ToString("d MMM yyyy"),
                            ApprovalGroup = taskInstance.UserGroup.Name,
                            Comments = taskInstance.WorkflowInstance.AuthorComment,
                            ActiveTask = taskInstance.WorkflowInstance.StatusName
                        };

                        if (_userId != -1 && includeActionLinks)
                        {
                            item.ShowActionLink = ShowActionLink(taskInstance, _userId);
                        }

                        var coordTaskInstance = taskInstance.WorkflowInstance.TaskInstances.First(ti => ti._Type == TaskType.CoordinatorApproval);

                        if (coordTaskInstance._Status == TaskStatus.Approved)
                        {
                            item.CoordinatedBy = coordTaskInstance.ActionedByUser.Name;
                            item.CoordinatedOn = coordTaskInstance.CompletedDate.Value.ToString("d MMM yyyy");
                            item.CoordinatorComments = coordTaskInstance.Comment;
                        }

                        workflowItems.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                var x = e.Message;
            }

            return workflowItems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_instance"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        private dynamic GetProcess(WorkflowInstancePoco _instance, Database db)
        {
            if (_instance._Type == WorkflowType.Publish)
            {
                return new DocumentPublishProcess(db);
            }
            return new DocumentUnpublishProcess(db);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskInstance"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        private bool ShowActionLink(WorkflowTaskInstancePoco taskInstance, int currentUserId)
        {
            return taskInstance.UserGroup.IsMember(currentUserId);
        }

        private WorkflowInstancePoco GetInstance(string taskId)
        {
            var _instance = PocoRepository.InstanceByTaskId(int.Parse(taskId));

            // TODO -> fix this
            var tasks = PocoRepository.TasksAndGroupByInstanceId(_instance.Guid);

            if (tasks.Any())
            {
                _instance.TaskInstances = tasks;
            }

            return _instance;
        }
    }
}