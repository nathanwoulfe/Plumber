using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web;
using Workflow.Extensions;
using Workflow.Models;

namespace Workflow.Helpers
{
    public class Utility
    {
        private static UmbracoHelper _helper = new UmbracoHelper(UmbracoContext.Current);
        private static IUserService _us = ApplicationContext.Current.Services.UserService;
        private static IContentTypeService _cts = ApplicationContext.Current.Services.ContentTypeService;
        private static IContentService _cs = ApplicationContext.Current.Services.ContentService;
        private static PocoRepository _pr = new PocoRepository();

        public static IPublishedContent GetNode(int id)
        {
            var n = _helper.TypedContent(id);
            if (n != null) return n;

            var c = _cs.GetById(id);

            return c?.ToPublishedContent();
        }

        public static string GetNodeName(int id)
        {
            var n = _helper.TypedContent(id);
            if (n == null)
            {
                var c = _cs.GetById(id);
                if (c != null)
                {
                    return c.Name;
                }
                return MagicStrings.NoNode;
            }
            return n.Name;
        }

        public static bool GetNodeStatus(int id)
        {
            return _pr.InstancesByNodeAndStatus(id, new List<int> { (int)WorkflowStatus.PendingApproval }).Any();
        }

        public static IUser GetUser(int id)
        {
            return _us.GetUserById(id);
        }

        public static IContentType GetContentType(int id)
        {
            return _cts.GetContentType(id);
        }

        public static IUser GetCurrentUser()
        {
            return UmbracoContext.Current.Security.CurrentUser;
        }

        public static bool IsTypeOfAdmin(string utAlias)
        {
            return utAlias == "admin" || utAlias == "siteadmin";
        }

        public static string PascalCaseToTitleCase(string str)
        {
            if (str != null)
            {
                return Regex.Replace(str, "([A-Z]+?(?=(([A-Z]?[a-z])|$))|[0-9]+)", " $1").Trim();
            }
            return null;
        }

        public static WorkflowSettingsPoco GetSettings()
        {
            return _pr.GetSettings();
        }

        /// <summary>Checks whether the email address is valid.</summary>
        /// <param name="email">the email address to check</param>
        /// <returns>true if valid, false otherwise.</returns>
        public static bool IsValidEmailAddress(string email)
        {
            try
            {
                var m = new MailAddress(email);
                return m.Address == email;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Builds workflow instance details markup.
        /// </summary>
        /// <param name="instance">The workflow instance to include in the list.</param>
        /// <param name="includeAction">true if the Action link should be included for those who have access to it.</param>
        /// <param name="includeCancel">true if the Cancel link should be included for those who have access to it.</param>
        /// <param name="includeComments">true if comments should be included in the details</param>
        /// <returns>HTML tr inner html definition</returns>
        public static string BuildProcessSummary(WorkflowInstancePoco instance, bool includeAction, bool includeCancel, bool includeComments)
        {
            var result = instance.TypeDescription + " requested by " + instance.AuthorUser.Name + " on " + instance.CreatedDate.ToString("dd/MM/yy") + " - " + instance.Status + "<br/>";

            if (includeComments && !string.IsNullOrEmpty(instance.AuthorComment))
            {
                result += "&nbsp;&nbsp;Comment: <i>" + instance.AuthorComment + "</i>";
            }
            result += "<br/>";
            
            foreach (var taskInstance in instance.TaskInstances)
            {
                if (taskInstance.Status == (int)TaskStatus.PendingApproval)
                {
                    result += BuildActiveTaskSummary(taskInstance, includeAction, includeCancel, false) + "<br/>";
                }
                else
                {
                    result += BuildInactiveTaskSummary(taskInstance, includeComments) + "<br/>";
                }
            }

            return result + "<br/>";
        }

        /// <summary>
        /// Creates a list of workflow task instances to be reviewed / actioned.
        /// </summary>
        /// <param name="taskInstances">Active task instances.</param>
        /// <param name="includeAction">true if the Action link should be included for those who have access to it.</param>
        /// <param name="includeCancel">true if the Cancel link should be included for those who have access to it.</param>
        /// <param name="includeEdit">true if the Edit icon should be included.</param>
        /// <returns>html markup describing a table of instance details</returns>
        public static string BuildActiveTasksList(List<WorkflowTaskInstancePoco> taskInstances, bool includeAction, bool includeCancel, bool includeEdit)
        {
            var result = "";

            if (taskInstances != null && taskInstances.Count > 0)
            {
                result += "<table style=\"workflowTaskList\">";
                result += "<tr><th>Type</th><th>Page</th><th>Requested by</th><th>On</th><th>Approver</th><th>Comments</th></tr>";

                result = taskInstances.Aggregate(result, (current, taskInstance) => current + ("<tr>" + BuildActiveTaskSummary(taskInstance, includeAction, includeCancel, includeEdit) + "</tr>"));

                result += "</table>";
            }
            else
            {
                result += "&nbsp;None.<br/><br/>";
            }

            return result;
        }

        /// <summary>
        /// Create html markup for an active workflow task including links to action, cancel, view, difference it.
        /// </summary>
        /// <param name="taskInstance">The task instance.</param>
        /// <param name="includeAction">true if the Action link should be included for those who have access to it.</param>
        /// <param name="includeCancel">true if the Cancel link should be included for those who have access to it.</param>
        /// <param name="includeEdit">true if the Edit icon should be included.</param>
        /// <returns>HTML markup describing an active task instance.</returns>
        public static string BuildActiveTaskSummary(WorkflowTaskInstancePoco taskInstance, bool includeAction, bool includeCancel, bool includeEdit)
        {
            var result = "";

            // Get the node from the cache if it's already published, otherwise look up the document from the DB
            var docId = taskInstance.WorkflowInstance.NodeId;
            var docTitle = "";
            var pageEditLink = "";

            var docUrl = GetDocPreviewUrl(docId);

            if (includeEdit)
            {
                pageEditLink = "<img alt=\"Edit\" title=\"Edit this document\" style=\"float:right\" src=\"../../images/edit.png\" onClick=\"window.open('" + "');\">";
            }

            var pageViewLink = "<a  target=\"_blank\" href=\"" + docUrl + "\">" + docTitle + "</a>";

            var createdDate = taskInstance.CreatedDate.ToString("dd/MM/yy");
            var authorText = taskInstance.WorkflowInstance.AuthorUser.Name;
            var approverText = "<a title='" + taskInstance.UserGroup.UsersSummary + "'>" + taskInstance.UserGroup.Name + "</a>";

            result += "<td>" + taskInstance.WorkflowInstance.TypeDescription + "</td><td><div>" + pageViewLink + "&nbsp" + pageEditLink + "</div></td><td>" + authorText + "</td><td>" + createdDate + "</td><td>" + approverText +
                "</td><td><small>" + taskInstance.WorkflowInstance.AuthorComment + "</small></td>";

            return result;
        }

        /// <summary>
        /// Create simple html markup for an inactive workflow task.
        /// </summary>
        /// <param name="taskInstance">The task instance.</param>
        /// <param name="includeComments">true if the comments should be included..</param>
        /// <returns>HTML markup describing an active task instance.</returns>
        public static string BuildInactiveTaskSummary(WorkflowTaskInstancePoco taskInstance, bool includeComments)
        {
            var result = taskInstance.TypeName;

            switch (taskInstance.Status)
            {
                case (int) TaskStatus.Approved:
                case (int) TaskStatus.Rejected:
                case (int) TaskStatus.Cancelled:
                    if (taskInstance.CompletedDate != null)
                    {
                        result += ": " + taskInstance.Status + " by " + taskInstance.ActionedByUser.Name + " on " + taskInstance.CompletedDate.Value.ToString("dd/MM/yy");
                    }
                    if (includeComments && !string.IsNullOrEmpty(taskInstance.Comment))
                    {
                        result += "<br/>&nbsp;&nbsp;Comment: <i>" + taskInstance.Comment + "</i>";
                    }
                    break;
                case (int) TaskStatus.NotRequired:
                    result += ": Not Required";
                    break;
            }

            return result;
        }

        public static string GetUrlPrefix()
        {
            if (HttpContext.Current == null)
                return "";

            var absUri = HttpContext.Current.Request.Url.AbsoluteUri.ToLower();
            return absUri.Substring(0, absUri.IndexOf("/umbraco", StringComparison.Ordinal));
        }

        public static string GetDocPreviewUrl(int docId)
        {
            return GetUrlPrefix() + "/umbraco/dialogs/preview.aspx?id=" + docId;
        }

        public static string GetDocPublishedUrl(int docId)
        {
            return GetUrlPrefix() + umbraco.library.NiceUrl(docId);
        }

        public static bool IUserCanAdminWorkflow(IUser user)
        {
            return IsTypeOfAdmin(user.UserType.Alias);
        }

        public static string BuildEmailSubject(EmailType emailType, WorkflowInstancePoco instance)
        {
            return WorkflowInstancePoco.EmailTypeName(emailType) + " - " + instance.Node.Name + " (" + instance.TypeDescription + ")";

        }

    }
}
