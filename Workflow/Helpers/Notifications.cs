using System;
using System.Linq;
using System.Net.Mail;
using Umbraco.Core.Models.Membership;
using Workflow.Models;

namespace Workflow
{
    public class Notifications
    {
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
            try
            {
                string docTitle = instance.Node.Name;
                string docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

                WorkflowTaskInstancePoco coordTaskInstance = instance.TaskInstances.FirstOrDefault(ti => ti._Type == TaskType.Approve);
                WorkflowTaskInstancePoco finalTaskInstance = instance.TaskInstances.FirstOrDefault(ti => ti._Type == TaskType.Publish);

                MailAddressCollection to = new MailAddressCollection();
                string email = Helpers.GetSettings().Email;
                string subject = "";
                string body = "";

                switch (emailType)
                {
                    case EmailType.ApprovalRequest:
                        to = coordTaskInstance.UserGroup.PreferredEmailAddresses();
                        body = string.Format(EmailApprovalRequestString,
                            coordTaskInstance.UserGroup.Name, docUrl, docTitle, instance.AuthorComment, instance.AuthorUser.Name, instance.TypeDescription);

                        break;

                    case EmailType.ApprovalRejection:
                        to.Add(instance.AuthorUser.Email);
                        body = string.Format(EmailRejectedString,
                            instance.AuthorUser.Name, docUrl, docTitle, coordTaskInstance.Comment, coordTaskInstance.ActionedByUser.Name, instance.TypeDescription.ToLower());

                        break;

                    case EmailType.ApprovedAndCompleted:
                        to = coordTaskInstance.UserGroup.PreferredEmailAddresses();
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
                        to = coordTaskInstance.UserGroup.PreferredEmailAddresses();
                        to.Add(instance.AuthorUser.Email);

                        docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

                        body = string.Format(EmailApprovedString,
                            instance.AuthorUser.Name, docUrl, docTitle, instance.TypeDescriptionPastTense.ToLower()) + "<br/>";

                        body += Helpers.BuildProcessSummary(instance, false, false, true);

                        break;

                    case EmailType.WorkflowCancelled:
                        string reason = "";
                        IUser cancelledBy;
                        // Get the emails for all usergroups for the tasks that have been raised as part of this workflow.
                        to = coordTaskInstance.UserGroup.PreferredEmailAddresses();
                        if (finalTaskInstance != null)
                        {
                            cancelledBy = finalTaskInstance.ActionedByUser;
                            to.Union(finalTaskInstance.UserGroup.PreferredEmailAddresses());
                            reason = finalTaskInstance.Comment;
                        }
                        else
                        {
                            cancelledBy = coordTaskInstance.ActionedByUser;
                            reason = coordTaskInstance.Comment;
                        }
                        // include the initiator email
                        to.Add(new MailAddress(instance.AuthorUser.Email));
                        body = string.Format(EmailCancelledString,
                            "Web User", instance.TypeDescription, docUrl, docTitle, cancelledBy.Name, reason);
                        break;
                }

                // Add a footer with information about having to login to umbraco first and listing the compatible browsers.
                string head = "<head><title>" + subject + "</title></head >";
                string html = "<!DOCTYPE HTML SYSTEM><html>" + head + "<body><font face=\"verdana\" size=\"2\">" + body + "</font></body></html>";

                subject = Helpers.BuildEmailSubject(emailType, instance);

                SmtpClient client = new SmtpClient();

                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(email);
                foreach (MailAddress address in to)
                {
                    msg.To.Add(address);
                }
                msg.To.Add(new MailAddress("mail@testdomain.com"));
                msg.Subject = subject;
                msg.Body = html;
                msg.IsBodyHtml = true;

                client.Send(msg);

                //Log.Debug("Sent email to " + to + " subject " + subject + " body " + body);
            }
            catch (Exception ex)
            {
                //Log.Error("Error sending email type " + emailType + " for process instance " + instance.Id, ex);
            }
        }
    }
}
