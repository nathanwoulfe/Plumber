using System;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using log4net;
using Workflow.Models;

namespace Workflow.Helpers
{
    public class Notifications
    {
        private static readonly PocoRepository Pr = new PocoRepository();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// TODO: these should come from a config file rather than static strings...
        /// </summary>
        private const string EmailApprovalRequestString = "Dear {0},<br/><br/>Please review the following page for {5} approval: <a href=\"{1}\">{2}</a> *<br/><br/>Comment: {3}<br/><br/>Thanks,<br/>{4}";
        private const string EmailApprovedString = "Dear {0},<br/>The following document's workflow has been approved and the document {3}: <a href=\"{1}\">{2}</a> *<br/>";
        private const string EmailRejectedString = "Dear {0},<br/>The {5} workflow was rejected by {4}: <a href=\"{1}\">{2}</a> *<br/>Comment: {3}";
        private const string EmailCancelledString = "Dear {0},<br/>{1} workflow has been cancelled for the following page: <a href=\"{2}\">{3}</a> * by {4}.<br/> Reason: {5}.";

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

                        body += Utility.BuildProcessSummary(instance);

                        break;

                    case EmailType.ApprovedAndCompletedForScheduler:
                        to = emailsForAllTaskUsers;
                        to.Add(instance.AuthorUser.Email);

                        docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

                        body = string.Format(EmailApprovedString,
                                   instance.AuthorUser.Name, docUrl, docTitle,
                                   instance.TypeDescriptionPastTense.ToLower()) + "<br/>";

                        body += Utility.BuildProcessSummary(instance);

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

                // Add a footer with information about having to login to umbraco first and listing the compatible browsers.
                var head = "<head><title>" + subject + "</title></head >";
                var html = "<!DOCTYPE HTML SYSTEM><html>" + head + "<body><font face=\"verdana\" size=\"2\">" + body +
                           "</font></body></html>";

                subject = Utility.BuildEmailSubject(emailType, instance);

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
    }
}
