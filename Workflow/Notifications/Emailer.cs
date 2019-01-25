using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
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

        private const string EmailApprovalRequestString = "Dear {0},<br/><br/>Please review the following page for {5} approval: <a href=\"{1}\">{2}</a><br/><br/>Comment: {3}<br/><br/>{6}Thanks,<br/>{4}";
        private const string EmailApprovedString = "Dear {0},<br/><br/>The following document's workflow has been approved and the document {3}: <a href=\"{1}\">{2}</a><br/>";
        private const string EmailRejectedString = "Dear {0},<br/><br/>The {5} workflow was rejected by {4}: <a href=\"{1}\">{2}</a><br/><br/>Comment: {3}";
        private const string EmailCancelledString = "Dear {0},<br/><br/>{1} workflow has been cancelled for the following page: <a href=\"{2}\">{3}</a> by {4}.<br/><br/>Reason: {5}.";
        private const string EmailErroredString = "Dear {0},<br/><br/>{1} workflow encountered a publishing error when attempting to publish the following page: <a href=\"{2}\">{3}</a>.<br/><br/>Error: {4}.<br/><br/>Your changes have been saved, please re-request publishing.";

        private const string EmailOfflineApprovalString = "<a href=\"{0}/workflow-preview/{1}/{2}/{3}/{4}\">Offline approval</a> is permitted for this change (no login required).<br/><br/>";

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
        public async void Send(WorkflowInstancePoco instance, EmailType emailType, string errorDetail = "")
        {
            if (!_settings.SendNotifications) return;

            if (!instance.TaskInstances.Any())
            {
                instance.TaskInstances = _tasksService.GetTasksWithGroupByInstanceGuid(instance.Guid);
            }

            if (!instance.TaskInstances.Any())
            {
                Log.Error($"Notifications not sent - no tasks exist for instance { instance.Id }");
                return;
            }

            try
            {
                WorkflowTaskPoco[] flowTasks = instance.TaskInstances.OrderBy(t => t.ApprovalStep).ToArray();

                // always take get the emails for all previous users, sometimes they will be discarded later
                // easier to just grab em all, rather than doing so conditionally
                List<string> emailsForAllTaskUsers = new List<string>();

                // in the loop, also store the last task to a variable, and keep the populated group
                var taskIndex = 0;
                int taskCount = flowTasks.Length;

                foreach (WorkflowTaskPoco task in flowTasks)
                {
                    taskIndex += 1;

                    UserGroupPoco group = await _groupService.GetPopulatedUserGroupAsync(task.GroupId);
                    if (group == null) continue;

                    emailsForAllTaskUsers.AddRange(group.PreferredEmailAddresses());
                    if (taskIndex != taskCount) continue;

                    _finalTask = task;
                    _finalTask.UserGroup = group;
                }

                if (_finalTask == null)
                {
                    Log.Error("No valid task found for email notifications");
                    return;
                }
                
                // populate list of recipients
                List<string> to = GetRecipients(emailType, instance, emailsForAllTaskUsers);
                if (!to.Any()) return;

                string body = GetBody(emailType, instance, to.Count == 1, out string typeDescription, errorDetail);

                var client = new SmtpClient();
                var msg = new MailMessage
                {
                    Subject = $"{emailType.ToString().ToTitleCase()} - {instance.Node.Name} ({typeDescription})",
                    IsBodyHtml = true,
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
                        string offlineString = string.Format(EmailOfflineApprovalString, _settings.SiteUrl, instance.NodeId,
                            user.UserId, _finalTask.Id, instance.Guid);

                        body = string.Format(EmailApprovalRequestString,
                            user.User.Name, docUrl, docTitle, instance.AuthorComment,
                            instance.AuthorUser.Name, typeDescription, offlineString);
                 
                        msg.To.Clear();
                        msg.To.Add(user.User.Email);
                        msg.Body = string.Format(EmailBody, msg.Subject, body);

                        client.Send(msg);
                    }
                }
                else
                {
                    msg.To.Add(string.Join(",", to.Distinct()));
                    msg.Body = string.Format(EmailBody, msg.Subject, body);

                    client.Send(msg);
                }

                Log.Info($"Email notifications sent for task { _finalTask.Id }, to { msg.To }");
            }
            catch (Exception e)
            {
                Log.Error($"Error sending notifications for task { _finalTask.Id }", e);
            }
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
        private string GetBody(EmailType emailType, WorkflowInstancePoco instance, bool useGenericName, out string typeDescription, string errorDetail = "")
        {
            var body = "";

            typeDescription = instance.WorkflowType.Description(instance.ScheduledDate);
            string typeDescriptionPast = instance.WorkflowType.DescriptionPastTense(instance.ScheduledDate);
            string docTitle = instance.Node.Name;
            string docUrl = UrlHelpers.GetFullyQualifiedContentEditorUrl(instance.NodeId);

            switch (emailType)
            {
                case EmailType.ApprovalRequest:
                    body = string.Format(EmailApprovalRequestString,
                        useGenericName ? "Umbraco user" : _finalTask.UserGroup.Name, docUrl, docTitle, instance.AuthorComment,
                        instance.AuthorUser.Name, typeDescription, string.Empty);
                    break;

                case EmailType.ApprovalRejection:
                    body = string.Format(EmailRejectedString,
                        "Umbraco user", docUrl, docTitle, _finalTask.Comment,
                        _finalTask.ActionedByUser.Name, typeDescription.ToLower());

                    break;

                case EmailType.ApprovedAndCompleted:
                    if (instance.WorkflowType == WorkflowType.Publish)
                    {
                        IPublishedContent n = _utility.GetPublishedContent(instance.NodeId);
                        docUrl = UrlHelpers.GetFullyQualifiedSiteUrl(n.Url);
                    }

                    body = string.Format(EmailApprovedString,
                               "Umbraco user", docUrl, docTitle,
                               typeDescriptionPast.ToLower()) + "<br/>";

                    body += instance.BuildProcessSummary();

                    break;

                case EmailType.ApprovedAndCompletedForScheduler:
                    body = string.Format(EmailApprovedString,
                               "Umbraco user", docUrl, docTitle,
                               typeDescriptionPast.ToLower()) + "<br/>";

                    body += instance.BuildProcessSummary();

                    break;

                case EmailType.WorkflowCancelled:
                    body = string.Format(EmailCancelledString,
                        "Umbraco user", typeDescription, docUrl, docTitle, _finalTask.ActionedByUser.Name, _finalTask.Comment);
                    break;

                case EmailType.WorkflowErrored:
                    body = string.Format(EmailErroredString, instance.AuthorUser.Name, typeDescription, docUrl,
                        docTitle, errorDetail);

                    break;

                case EmailType.SchedulerActionCancelled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(emailType), emailType, null);
            }

            return body;
        }

        /// <summary>
        /// Builds the recipient list based on the email type
        /// </summary>
        /// <param name="emailType"></param>
        /// <param name="instance"></param>
        /// <param name="emailsForAllTaskUsers"></param>
        /// <returns></returns>
        private List<string> GetRecipients(EmailType emailType, WorkflowInstancePoco instance, List<string> emailsForAllTaskUsers)
        {
            List<string> to = new List<string>();
            switch (emailType)
            {
                case EmailType.ApprovalRequest:
                    to = _finalTask.UserGroup.PreferredEmailAddresses();
                    break;
                    
                case EmailType.ApprovedAndCompleted:
                    to = emailsForAllTaskUsers;
                    to.Add(instance.AuthorUser.Email);

                    //Notify web admins
                    to.Add(_settings.Email);

                    break;

                case EmailType.ApprovalRejection:
                case EmailType.ApprovedAndCompletedForScheduler:
                case EmailType.WorkflowCancelled:
                    to = emailsForAllTaskUsers;

                    // include the initiator email
                    to.Add(instance.AuthorUser.Email);
                    break;

                case EmailType.WorkflowErrored:
                    to.Add(instance.AuthorUser.Email);

                    //Notify web admins
                    to.Add(_settings.Email);
                    break;

                case EmailType.SchedulerActionCancelled:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(emailType), emailType, null);
            }

            return to;
        }
    }
}
