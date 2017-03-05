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
using Umbraco.Core.Models;
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
        private static PocoRepository _pr = new PocoRepository();
        private List<UserGroupPermissionsPoco> perms = new List<UserGroupPermissionsPoco>();

        /// <summary>
        /// Returns all tasks currently in workflow processes
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        public HttpResponseMessage GetPendingTasks()
        {
            try
            {
                var taskInstances = _pr.GetPendingTasks((int)TaskStatus.PendingApproval);
                var workflowItems = BuildWorkflowItemList(taskInstances, -1, false);
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.OK,
                    data = workflowItems
                });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.InternalServerError,
                    data = e.Message
                });
            }
        }

        /// <summary>
        /// Returns all tasks
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        public HttpResponseMessage GetAllTasks()
        {
            try
            {
                var taskInstances = _pr.GetAllTasks();
                var workflowItems = BuildWorkflowItemList(taskInstances, -1, false);
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.OK,
                    data = workflowItems
                });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.InternalServerError,
                    data = e.Message
                });
            }
        }

        /// <summary>
        /// Returns all tasks
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        public HttpResponseMessage GetAllInstances()
        {
            try
            {
                var instances = _pr.GetAllInstances();
                var workflowInstances = BuildWorkflowInstanceList(instances);
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.OK,
                    data = workflowInstances
                });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.InternalServerError,
                    data = e.Message
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetNodeTasks(string id)
        {
            try
            {
                var taskInstances = _pr.TasksByNode(id);
                var workflowItems = BuildWorkflowItemList(taskInstances, -1, false);
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.OK,
                    data = workflowItems
                });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.InternalServerError,
                    data = e.Message
                });
            }
        }

        /// <summary>
        /// Gets all tasks requiring actioning by the current user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type">0 - tasks, 1 - submissions</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFlowsForUser(int userId, int type = 0)
        {

            var workflowItems = new List<WorkflowTask>();

            try
            {
                var taskInstances = type == 0 ? _pr.TasksForUser(userId, (int)TaskStatus.PendingApproval) : _pr.SubmissionsForUser(userId, (int)TaskStatus.PendingApproval);
                workflowItems = BuildWorkflowItemList(taskInstances, userId);

                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.OK,
                    data = workflowItems
                });
            }
            catch (Exception ex)
            {
                log.Error("Error trying to build user workflow tasks list for user " + Helpers.GetUser(userId).Name, ex);
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.InternalServerError,
                    data = string.Concat("Error trying to build user workflow tasks list for user ", Helpers.GetUser(userId).Name, ": ", ex)
                });
            }
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

            var publishedVersion = Umbraco.TypedContent(nodeId); // most recent published version
            var revisedVersion = Services.ContentService.GetById(_nodeId); // current version from database

            var differences = new DifferencesResponseItem
            {
                CurrentVersionPubDate = publishedVersion.UpdateDate.ToString("d MMM yyyy"),
                RevisedVersionPubDate = revisedVersion.UpdateDate.ToString("d MMM yyyy"),
            };

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
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="authorId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage InitiateWorkflow(InitiateWorkflowModel model)
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
                    process = null;
                }

                instance = process.InitiateWorkflow(int.Parse(model.NodeId), Helpers.GetCurrentUser().Id, model.Comment);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.BadRequest,
                    data = "Something went wrong " + e.Message
                });
            }

            if (instance != null)
            {
                var msg = string.Empty;

                switch (instance._Status)
                {
                    case WorkflowStatus.PendingApproval:
                        msg = "Page submitted for approval";
                        break;
                    case WorkflowStatus.Approved:
                        msg = "Workflow complete";
                        break;
                }
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.OK,
                    data = msg
                });
            }

            return Request.CreateResponse(new
            {
                status = HttpStatusCode.BadRequest,
                data = "Something went wrong"
            });
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
                WorkflowApprovalProcess process = GetProcess(_instance.Type);

                _instance = process.ActionWorkflow(
                    _instance,
                    WorkflowAction.Approve,
                    Helpers.GetCurrentUser().Id,
                    comment
                );

                string msg = string.Empty;

                switch (_instance._Status)
                {
                    case WorkflowStatus.PendingApproval:
                        msg = "Approval completed successfully. Page will be " + _instance.TypeDescriptionPastTense.ToLower() + " workflow completion.";
                        break;
                    case WorkflowStatus.Approved:
                        msg = "Workflow approved successfully, page has been " + _instance.TypeDescriptionPastTense.ToLower();
                        break;
                }

                var respMessage = new WorkflowResponseItem
                {
                    Message = msg,
                    Type = _instance._Type
                };

                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.OK,
                    data = respMessage
                });
            }
            catch (Exception ex)
            {
                string msg = "An error occurred processing the approval: " + ex.Message + ex.StackTrace;
                log.Error(msg + " for workflow " + _instance.Id, ex);

                return Request.CreateResponse(new {
                    status = HttpStatusCode.BadRequest,
                    data = new HttpError(msg)
                });
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
                WorkflowApprovalProcess process = GetProcess(_instance.Type);

                _instance = process.ActionWorkflow(
                    _instance,
                    WorkflowAction.Reject,
                    Helpers.GetCurrentUser().Id,
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

                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.BadRequest,
                    data = new HttpError(msg)
                });
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
                WorkflowApprovalProcess process = GetProcess(_instance.Type);

                _instance = process.CancelWorkflow(
                    _instance,
                    Helpers.GetCurrentUser().Id,
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
        private List<WorkflowTask> BuildWorkflowItemList(List<WorkflowTaskInstancePoco> taskInstances, int _userId = -1, bool includeActionLinks = true, WorkflowInstancePoco instance = null)
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
                        Type = useThisInstance.TypeDescription,
                        NodeId = useThisInstance.NodeId,
                        TaskId = useThisInstance.Id,
                        ApprovalGroupId = taskInstance.UserGroup.GroupId,
                        NodeName = useThisInstance.Node.Name,
                        RequestedBy = useThisInstance.AuthorUser.Name,
                        RequestedOn = taskInstance.CreatedDate.ToString("d MMM yyyy"),
                        ApprovalGroup = taskInstance.UserGroup.Name,
                        Comments = taskInstance.Comment != null ? taskInstance.Comment : useThisInstance.AuthorComment != null ? useThisInstance.AuthorComment : string.Empty,
                        ActiveTask = useThisInstance.StatusName,
                        Permissions = perms,
                        CurrentStep = taskInstance.ApprovalStep
                    };

                    if (_userId != -1 && includeActionLinks)
                    {
                        item.ShowActionLink = ShowActionLink(taskInstance, _userId);
                    }

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
                    var model = new WorkflowInstance
                    {
                        Type = instance.TypeDescription,
                        Status = instance.StatusName,
                        NodeId = instance.NodeId,
                        NodeName = instance.Node.Name,
                        RequestedBy = instance.AuthorUser.Name,
                        RequestedOn = instance.CreatedDate.ToString("d MMM yyyy"),
                        Tasks = BuildWorkflowItemList(instance.TaskInstances.ToList(), -1, false, instance).OrderByDescending(x => x.CurrentStep).ToList()
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
            perms = _pr.PermissionsForNode(node.Id, node.ContentType.Id);

            // return them if they exist, otherwise check the parent
            if (!perms.Any() && node.Level != 1)
            {
                GetPermissionsForNode(node.Parent);
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
        /// <param name="taskInstance"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        private bool ShowActionLink(WorkflowTaskInstancePoco taskInstance, int currentUserId)
        {
            return taskInstance.UserGroup.IsMember(currentUserId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        private WorkflowInstancePoco GetInstance(string taskId)
        {
            var _instance = _pr.InstanceByTaskId(int.Parse(taskId));

            // TODO -> fix this
            var tasks = _pr.TasksAndGroupByInstanceId(_instance.Guid);

            if (tasks.Any())
            {
                _instance.TaskInstances = tasks;
            }

            return _instance;
        }
    }
}