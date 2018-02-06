using System;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using log4net;
using Workflow.Helpers;
using Workflow.Models;

namespace Workflow
{
    public class Notifications
    {
        private static readonly PocoRepository Pr = new PocoRepository();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// TODO: these should come from a config file rather than static strings...
        /// </summary>
        private const string EmailApprovalRequestString = "Dear {0},<br/><br/>Please review the following page for {5} approval: <a href=\"{1}\">{2}</a><br/><br/>Comment: {3}<br/><br/>Thanks,<br/>{4}";
        private const string EmailApprovedString = "Dear {0},<br/>The following document's workflow has been approved and the document {3}: <a href=\"{1}\">{2}</a><br/>";
        private const string EmailRejectedString = "Dear {0},<br/>The {5} workflow was rejected by {4}: <a href=\"{1}\">{2}</a><br/>Comment: {3}";
        private const string EmailCancelledString = "Dear {0},<br/>{1} workflow has been cancelled for the following page: <a href=\"{2}\">{3}</a> by {4}.<br/> Reason: {5}.";

        /// <summary>
        /// Sends an email notification out for the workflow process
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="emailType">the type of email to be sent</param>
        public static void Send(WorkflowInstancePoco instance, EmailType emailType)
        {
            bool? doSend = Pr.GetSettings().SendNotifications;
            if (doSend != true) return;

            try
            {
                var docTitle = instance.Node.Name;
                var docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

                var flowTasks = instance.TaskInstances.OrderBy(t => t.ApprovalStep);
                var userIdToExclude = Utility.GetSettings().FlowType != (int) FlowType.All
                    ? instance.AuthorUserId
                    : int.MinValue;

                var emailsForAllTaskUsers = new MailAddressCollection();
                foreach (var task in flowTasks)
                {
                    var group = task.UserGroup ?? Pr.PopulatedUserGroup(task.GroupId).First();
                    emailsForAllTaskUsers.Union(group.PreferredEmailAddresses(userIdToExclude));
                }

                var to = new MailAddressCollection();
                var email = Utility.GetSettings().Email;
                var subject = "";
                var body = "";

                switch (emailType)
                {
                    case EmailType.ApprovalRequest:
                        to = flowTasks.Last().UserGroup.PreferredEmailAddresses(userIdToExclude);
                        body = string.Format(EmailApprovalRequestString,
                            flowTasks.Last().UserGroup.Name, docUrl, docTitle, instance.AuthorComment,
                            instance.AuthorUser.Name, instance.TypeDescription);

                        break;

                    case EmailType.ApprovalRejection:
                        to = emailsForAllTaskUsers;
                        to.Add(instance.AuthorUser.Email);
                        body = string.Format(EmailRejectedString,
                            instance.AuthorUser.Name, docUrl, docTitle, flowTasks.Last().Comment,
                            flowTasks.Last().ActionedByUser.Name, instance.TypeDescription.ToLower());

                        break;

                    case EmailType.ApprovedAndCompleted:
                        to = emailsForAllTaskUsers;
                        to.Add(instance.AuthorUser.Email);

                        //Notify web admins
                        to.Add(email);

                        if (instance.WorkflowType == WorkflowType.Publish)
                        {
                            var n = Utility.GetNode(instance.NodeId);
                            docUrl = UrlHelpers.GetFullyQualifiedSiteUrl(n.Url);
                        }
                        else
                        {
                            docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);
                        }

                        body = string.Format(EmailApprovedString,
                                   instance.AuthorUser.Name, docUrl, docTitle,
                                   instance.TypeDescriptionPastTense.ToLower()) + "<br/>";

                        body += BuildProcessSummary(instance);

                        break;

                    case EmailType.ApprovedAndCompletedForScheduler:
                        to = emailsForAllTaskUsers;
                        to.Add(instance.AuthorUser.Email);

                        docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

                        body = string.Format(EmailApprovedString,
                                   instance.AuthorUser.Name, docUrl, docTitle,
                                   instance.TypeDescriptionPastTense.ToLower()) + "<br/>";

                        body += BuildProcessSummary(instance);

                        break;

                    case EmailType.WorkflowCancelled:
                        // Get the emails for all usergroups for the tasks that have been raised as part of this workflow.
                        to = emailsForAllTaskUsers;
                        var reason = flowTasks.Last().Comment;
                        var cancelledBy = flowTasks.Last().ActionedByUser;

                        // include the initiator email
                        to.Add(new MailAddress(instance.AuthorUser.Email));
                        body = string.Format(EmailCancelledString,
                            "Umbraco user", instance.TypeDescription, docUrl, docTitle, cancelledBy.Name, reason);
                        break;
                    case EmailType.SchedulerActionCancelled:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(emailType), emailType, null);
                }

                if (!to.Any()) return;

                var html = $"<!DOCTYPE HTML SYSTEM><html><head><title>{subject}</title></head><body><font face=\"verdana\" size=\"2\">{body}</font></body></html>";

                subject = BuildEmailSubject(emailType, instance);

                var client = new SmtpClient();
                var msg = new MailMessage();

                if (!string.IsNullOrEmpty(email))
                {
                    msg.From = new MailAddress(email);
                }

                foreach (var address in to)
                {
                    msg.To.Add(address);
                }

                msg.Subject = subject;
                msg.Body = html;
                msg.IsBodyHtml = true;

                client.Send(msg);
            }
            catch (Exception e)
            {
                Log.Error("Error sending notifications", e);
            }
        }

        /// <summary>
        /// Builds workflow instance details markup.
        /// </summary>
        /// <returns>HTML tr inner html definition</returns>
        private static string BuildProcessSummary(WorkflowInstancePoco instance)
        {
            string result = $"{instance.TypeDescription} requested by {instance.AuthorUser.Name} on {instance.CreatedDate.ToString("dd/MM/yy")} - {instance.StatusName}<br/>";

            if (!string.IsNullOrEmpty(instance.AuthorComment))
            {
                result += $"&nbsp;&nbsp;Comment: <i>{instance.AuthorComment}</i>";
            }
            result += "<br/>";

            var index = 1;

            foreach (var taskInstance in instance.TaskInstances)
            {
                result += BuildTaskSummary(taskInstance, index) + "<br/>";
                index += 1;
            }

            return result + "<br/>";
        }

        /// <summary>
        /// Create simple html markup for an inactive workflow task.
        /// </summary>
        /// <param name="taskInstance">The task instance.</param>
        /// <param name="index"></param>
        /// <returns>HTML markup describing an active task instance.</returns>
        private static string BuildTaskSummary(WorkflowTaskInstancePoco taskInstance, int index)
        {
            var result = "";

            switch (taskInstance.Status)
            {
                case (int)TaskStatus.Approved:
                case (int)TaskStatus.Rejected:
                case (int)TaskStatus.Cancelled:

                    if (taskInstance.CompletedDate != null)
                    {
                        result += $"Stage {index}: {taskInstance.StatusName} by {taskInstance.ActionedByUser.Name} on {taskInstance.CompletedDate.Value.ToString("dd/MM/yy")}";
                    }

                    if (!string.IsNullOrEmpty(taskInstance.Comment))
                    {
                        result += $"<br/>&nbsp;&nbsp;Comment: <i>{taskInstance.Comment}</i>";
                    }

                    break;

                case (int)TaskStatus.NotRequired:

                    result += $"Stage {index}: Not required";

                    break;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailType"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        private static string BuildEmailSubject(EmailType emailType, WorkflowInstancePoco instance)
        {
            return $"{WorkflowInstancePoco.EmailTypeName(emailType)} - {instance.Node.Name} ({instance.TypeDescription})";
        }
    }
}
