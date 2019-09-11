using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Umbraco.Core.Models;
using Workflow.Extensions;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Notifications
{
    public class Emailer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ITasksService _tasksService;
        private readonly IGroupService _groupService;

        private readonly WorkflowSettingsPoco _settings;
        private static WorkflowTaskPoco _finalTask;

        private readonly Utility _utility;

        private const string EmailApprovalRequestString = "Please review the following page for {2} approval: <a href=\"{0}\">{1}</a><br/><br/>";
        private const string EmailApprovedString = "The following document's workflow has been approved and the document {2}: <a href=\"{0}\">{1}</a><br/><br/>";
        private const string EmailRejectedString = "The {4} workflow on <a href=\"{0}\">{1}</a> was rejected by {3}<br/><br/>";
        private const string EmailCancelledString = "{0} workflow has been cancelled for the following page: <a href=\"{1}\">{2}</a> by {3}.<br/><br/>";
        private const string EmailErroredString = "{0} workflow encountered a publishing error when attempting to publish the following page: <a href=\"{1}\">{2}</a>.<br/><br/>Error: {3}.<br/><br/>Your changes have been saved, please re-request publishing.";

        private const string EmailOfflineApprovalString = "<br/><br/><a href=\"{0}/workflow-preview/{1}/{2}/{3}/{4}\">Offline approval</a> is permitted for this change (no login required).";

        private const string EmailBody = "<!DOCTYPE HTML SYSTEM><html><head><title>{0}</title></head><body><font face=\"verdana\" size=\"2\">{1}</font></body></html>";

        public Emailer()
        {
            _settings = new SettingsService().GetSettings();
            _tasksService = new TasksService();
            _groupService = new GroupService();

            _utility = new Utility();
        }

        /// <summary>
        /// Sends an email notification out for the workflow process
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="emailType">the type of email to be sent</param>
        /// <param name="errorDetail"></param>
        public async Task<string> Send(WorkflowInstancePoco instance, EmailType emailType, string errorDetail = "")
        {
            var msg = new MailMessage();

            if (!_settings.SendNotifications) return null;

            if (!instance.TaskInstances.Any())
            {
                instance.TaskInstances = _tasksService.GetTasksWithGroupByInstanceGuid(instance.Guid);
            }

            if (!instance.TaskInstances.Any())
            {
                Log.Error($"Notifications not sent - no tasks exist for instance { instance.Id }");
                return null;
            }

            try
            {
                WorkflowTaskPoco[] flowTasks = instance.TaskInstances.OrderBy(t => t.ApprovalStep).ToArray();

                // always take get the emails for all previous users, sometimes they will be discarded later
                // easier to just grab em all, rather than doing so conditionally
                var emailsForAllTaskUsers = new EmailRecipients();

                // in the loop, also store the last task to a variable, and keep the populated group
                var taskIndex = 0;
                int taskCount = flowTasks.Length;

                foreach (WorkflowTaskPoco task in flowTasks)
                {
                    taskIndex += 1;

                    UserGroupPoco group = await _groupService.GetPopulatedUserGroupAsync(task.GroupId);
                    if (group == null) continue;

                    emailsForAllTaskUsers.ToRecipients.AddRange(group.PreferredEmailAddresses());
                    emailsForAllTaskUsers.CCRecipients.AddRange(group.CCEmailAddresses());
                    if (taskIndex != taskCount) continue;

                    _finalTask = task;
                    _finalTask.UserGroup = group;
                }

                if (_finalTask == null)
                {
                    Log.Error("No valid task found for email notifications");
                    return null;
                }

                // populate list of recipients
                var to = GetRecipients(emailType, instance, emailsForAllTaskUsers);
                if (!to.ToRecipients.Any()) return null;

                string body = GetBody(emailType, instance, out string typeDescription, errorDetail);

                var client = new SmtpClient();

                msg = new MailMessage
                {
                    Subject = $"{emailType.ToString().ToTitleCase()} - {instance.Node.Name} ({typeDescription})",
                    IsBodyHtml = true,
                    Body = string.Format(EmailBody, msg.Subject, body)
                };

                if (_settings.Email.HasValue())
                {
                    msg.From = new MailAddress(_settings.Email);
                }

                // if offline is permitted, email group members individually as we need the user id in the url
                if (emailType == EmailType.ApprovalRequest && _finalTask.UserGroup.OfflineApproval)
                {
                    string docTitle = instance.Node.Name;
                    string docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

                    foreach (User2UserGroupPoco user in _finalTask.UserGroup.Users)
                    {
                        var msgBody = body + string.Format(EmailOfflineApprovalString, _settings.SiteUrl, instance.NodeId,
                            user.UserId, _finalTask.Id, instance.Guid);

                        msg.Body = string.Format(EmailBody, msg.Subject, msgBody);

                        msg.To.Clear();
                        msg.To.Add(user.User.Email);

                        client.Send(msg);
                    }
                }
                else
                {
                    msg.To.Add(string.Join(",", to.ToRecipients.Distinct()));
                    msg.CC.Add(string.Join(",", to.CCRecipients.Distinct()));
                    client.Send(msg);
                }

                Log.Info($"Email notifications sent for task { _finalTask.Id }, to { msg.To }");
            }
            catch (Exception e)
            {
                Log.Error($"Error sending notifications for task { _finalTask.Id }", e);
            }

            return msg.Body;
        }

        /// <summary>
        /// Builds the email body based on the email type
        /// </summary>
        /// <param name="emailType"></param>
        /// <param name="instance"></param>
        /// <param name="useGenericName"></param>
        /// <param name="typeDescription"></param>
        /// <param name="errorDetail"></param>
        /// <returns></returns>
        private string GetBody(EmailType emailType, WorkflowInstancePoco instance, out string typeDescription, string errorDetail = "")
        {
            var body = "";

            typeDescription = instance.WorkflowType.Description(instance.ScheduledDate);
            string typeDescriptionPast = instance.WorkflowType.DescriptionPastTense(instance.ScheduledDate);
            string docTitle = instance.Node.Name;
            string docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

            switch (emailType)
            {
                case EmailType.ApprovalRequest:
                    body = string.Format(EmailApprovalRequestString, docUrl, docTitle, typeDescription.ToLower());

                    break;

                case EmailType.ApprovalRejection:
                    body = string.Format(EmailRejectedString, docUrl, docTitle, _finalTask.Comment,
                        _finalTask.ActionedByUser.Name, typeDescription.ToLower());

                    break;

                case EmailType.ApprovedAndCompleted:
                    if (instance.WorkflowType == WorkflowType.Publish)
                    {
                        IPublishedContent n = _utility.GetPublishedContent(instance.NodeId);
                        docUrl = UrlHelpers.GetFullyQualifiedSiteUrl(n.Url);
                    }

                    body = string.Format(EmailApprovedString, docUrl, docTitle,
                               typeDescriptionPast.ToLower()) + "<br/>";

                    break;

                case EmailType.ApprovedAndCompletedForScheduler:
                    body = string.Format(EmailApprovedString, docUrl, docTitle,
                               typeDescriptionPast.ToLower()) + "<br/>";

                    break;

                case EmailType.WorkflowCancelled:
                    body = string.Format(EmailCancelledString, typeDescription, docUrl, docTitle, _finalTask.ActionedByUser.Name);

                    break;

                case EmailType.WorkflowErrored:
                    body = string.Format(EmailErroredString, typeDescription, docUrl, docTitle, errorDetail);

                    break;

                case EmailType.SchedulerActionCancelled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(emailType), emailType, null);
            }

            body += instance.BuildProcessSummary();

            return body;
        }

        /// <summary>
        /// Builds the recipient list based on the email type
        /// </summary>
        /// <param name="emailType"></param>
        /// <param name="instance"></param>
        /// <param name="emailsForAllTaskUsers"></param>
        /// <returns></returns>
        private EmailRecipients GetRecipients(EmailType emailType, WorkflowInstancePoco instance, EmailRecipients emailsForAllTaskUsers)
        {
            var recipients = new EmailRecipients();
            switch (emailType)
            {
                case EmailType.ApprovalRequest:
                    recipients.ToRecipients = _finalTask.UserGroup.PreferredEmailAddresses();
                    recipients.CCRecipients = _finalTask.UserGroup.CCEmailAddresses();
                    break;

                case EmailType.ApprovedAndCompleted:
                    recipients.ToRecipients = emailsForAllTaskUsers.ToRecipients;
                    recipients.CCRecipients = emailsForAllTaskUsers.CCRecipients;
                    recipients.ToRecipients.Add(instance.AuthorUser.Email);

                    //Notify web admins
                    recipients.ToRecipients.Add(_settings.Email);

                    break;

                case EmailType.ApprovalRejection:
                case EmailType.ApprovedAndCompletedForScheduler:
                case EmailType.WorkflowCancelled:
                    recipients.ToRecipients = emailsForAllTaskUsers.ToRecipients;
                    recipients.CCRecipients = emailsForAllTaskUsers.CCRecipients;

                    // include the initiator email
                    recipients.ToRecipients.Add(instance.AuthorUser.Email);
                    break;

                case EmailType.WorkflowErrored:
                    recipients.ToRecipients.Add(instance.AuthorUser.Email);

                    //Notify web admins
                    recipients.ToRecipients.Add(_settings.Email);
                    break;

                case EmailType.SchedulerActionCancelled:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(emailType), emailType, null);
            }

            return recipients;
        }
    }
}
