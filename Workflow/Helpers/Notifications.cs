using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Umbraco.Core.Models.Membership;
using Workflow.Models;

namespace Workflow
{
    public class Notifications
    {
        private static PocoRepository _pr = new PocoRepository();

        private static string EmailApprovalRequestString = "Dear {0},<br/><br/>Please review the following page for {5} approval: <a href=\"{1}\">{2}</a> *<br/><br/>Comment: {3}<br/><br/>Thanks,<br/>{4}";
        private static string EmailApprovedString = "Dear {0},<br/>The following document's workflow has been approved and the document {3}: <a href=\"{1}\">{2}</a> *<br/>";
        private static string EmailRejectedString = "Dear {0},<br/>The {5} workflow was rejected by {4}: <a href=\"{1}\">{2}</a> *<br/>Comment: {3}";
        private static string EmailCancelledString = "Dear {0},<br/>{1} workflow has been cancelled for the following page: <a href=\"{2}\">{3}</a> * by {4}.<br/> Reason: {5}.";  

        /// <summary>
        /// Sends an email notification out for the workflow process
        /// </summary>
        /// <param name="emailType">the type of email to be sent</param>
        public static void Send(WorkflowInstancePoco instance, EmailType emailType)
        {
            bool? doSend = _pr.GetSettings().SendNotifications;

            if (doSend != null && doSend == true)
            {
                try
                {
                    string docTitle = instance.Node.Name;
                    string docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

                    var flowTasks = instance.TaskInstances.OrderBy(t => t.ApprovalStep);
                    var userIdToExclude = Helpers.GetSettings().FlowType != (int)FlowType.All ? instance.AuthorUserId : int.MinValue;

                    var emailsForAllTaskUsers = new MailAddressCollection();
                    foreach (var task in flowTasks)
                    {
                        var group = task.UserGroup;
                        if (group == null)
                        {
                            group = _pr.PopulatedUserGroup(task.GroupId).First();
                        }

                        emailsForAllTaskUsers.Union(group.PreferredEmailAddresses(userIdToExclude));
                    }

                    MailAddressCollection to = new MailAddressCollection();
                    string email = Helpers.GetSettings().Email;
                    string subject = "";
                    string body = "";

                    switch (emailType)
                    {
                        case EmailType.ApprovalRequest:
                            to = flowTasks.Last().UserGroup.PreferredEmailAddresses(userIdToExclude);
                            body = string.Format(EmailApprovalRequestString,
                                flowTasks.Last().UserGroup.Name, docUrl, docTitle, instance.AuthorComment, instance.AuthorUser.Name, instance.TypeDescription);

                            break;

                        case EmailType.ApprovalRejection:
                            to = emailsForAllTaskUsers;
                            to.Add(instance.AuthorUser.Email);
                            body = string.Format(EmailRejectedString,
                                instance.AuthorUser.Name, docUrl, docTitle, flowTasks.Last().Comment, flowTasks.Last().ActionedByUser.Name, instance.TypeDescription.ToLower());

                            break;

                        case EmailType.ApprovedAndCompleted:
                            to = emailsForAllTaskUsers;
                            to.Add(instance.AuthorUser.Email);

                            //Notify web admins
                            to.Add(email);

                            if (instance._Type == WorkflowType.Publish)
                            {
                                var n = Helpers.GetNode(instance.NodeId);
                                docUrl = UrlHelpers.GetFullyQualifiedSiteUrl(n.Url);
                            }
                            else
                            {
                                docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);
                            }

                            body = string.Format(EmailApprovedString,
                                instance.AuthorUser.Name, docUrl, docTitle, instance.TypeDescriptionPastTense.ToLower()) + "<br/>";

                            body += Helpers.BuildProcessSummary(instance, false, false, true);

                            break;

                        case EmailType.ApprovedAndCompletedForScheduler:
                            to = emailsForAllTaskUsers;
                            to.Add(instance.AuthorUser.Email);

                            docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

                            body = string.Format(EmailApprovedString,
                                instance.AuthorUser.Name, docUrl, docTitle, instance.TypeDescriptionPastTense.ToLower()) + "<br/>";

                            body += Helpers.BuildProcessSummary(instance, false, false, true);

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
                    }

                    if (to.Any())
                    {

                        // Add a footer with information about having to login to umbraco first and listing the compatible browsers.
                        string head = "<head><title>" + subject + "</title></head >";
                        string html = "<!DOCTYPE HTML SYSTEM><html>" + head + "<body><font face=\"verdana\" size=\"2\">" + body + "</font></body></html>";

                        subject = Helpers.BuildEmailSubject(emailType, instance);

                        SmtpClient client = new SmtpClient();
                        MailMessage msg = new MailMessage();

                        if (!string.IsNullOrEmpty(email))
                        {
                            msg.From = new MailAddress(email);
                        }
                        foreach (MailAddress address in to)
                        {
                            msg.To.Add(address);
                        }
                        msg.To.Add(new MailAddress("mail@testdomain.com"));
                        msg.Subject = subject;
                        msg.Body = html;
                        msg.IsBodyHtml = true;

                        client.Send(msg);
                    }
                }
                catch (Exception ex) { }
            }
        }
    }
}
