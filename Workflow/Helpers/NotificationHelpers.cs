using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.UI;

namespace Workflow.Helpers
{
    /// <summary>
    /// Helper methods for working with Notifications and EventMessages
    /// </summary>
    /// <remarks>Adapated from Umbraco source to mimic default backoffice UI behaviors</remarks>
    internal static class NotificationHelpers
    {
        /// <summary>
        /// A helper method to map a collection of EventMessages to Notifications for display in the UI
        /// </summary>
        /// <remarks>
        /// From umbraco source -  Umbraco.Web.WebApi.Filters.AppendCurrentEventMessagesAttribute
        /// </remarks>
        public static IEnumerable<Notification> MapEventMessagesToNotifications(IEnumerable<EventMessage> eventMessages)
        {
            var notifications = new List<Notification>();

            if (eventMessages == null) return notifications;

            foreach (var eventMessage in eventMessages)
            {
                SpeechBubbleIcon msgType;
                switch (eventMessage.MessageType)
                {
                    case EventMessageType.Default:
                        msgType = SpeechBubbleIcon.Save;
                        break;
                    case EventMessageType.Info:
                        msgType = SpeechBubbleIcon.Info;
                        break;
                    case EventMessageType.Error:
                        msgType = SpeechBubbleIcon.Error;
                        break;
                    case EventMessageType.Success:
                        msgType = SpeechBubbleIcon.Success;
                        break;
                    case EventMessageType.Warning:
                        msgType = SpeechBubbleIcon.Warning;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                notifications.Add(new Notification
                {
                    Message = eventMessage.Message,
                    Header = eventMessage.Category,
                    NotificationType = msgType
                });
            }

            return notifications;
        }

        /// <summary>
        /// A helper method to mimic the default Umbraco notifications based on PublishStatus
        /// </summary>
        /// <remarks>
        /// From umbraco source - ContentController.PostSave -> ShowMessageForPublishStatus
        /// </remarks>
        public static IEnumerable<EventMessage> GetUmbracoDefaultEventMessages(PublishStatus status, ILocalizedTextService textService)
        {
            var messages = new List<EventMessage>();

            switch (status.StatusType)
            {
                case PublishStatusType.Success:
                case PublishStatusType.SuccessAlreadyPublished:
                    messages.Add(
                        new EventMessage(
                            textService.Localize("speechBubbles/editContentPublishedHeader"),
                            status.ContentItem.ExpireDate.HasValue
                                ? textService.Localize("speechBubbles/editContentPublishedWithExpireDateText", new[] { status.ContentItem.ExpireDate.Value.ToLongDateString(), status.ContentItem.ExpireDate.Value.ToShortTimeString() })
                                : textService.Localize("speechBubbles/editContentPublishedText")
                            ));
                    break;
                case PublishStatusType.FailedPathNotPublished:
                    messages.Add(
                        new EventMessage(
                            textService.Localize("publish"),
                            textService.Localize("publish/contentPublishedFailedByParent",
                                new[] { string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id) }).Trim(),
                            EventMessageType.Warning));
                    break;
                case PublishStatusType.FailedCancelledByEvent:
                    if (status.EventMessages.Count == 0) // only show default cancel notification if one wasn't already added
                    {
                        messages.Add(
                            new EventMessage(textService.Localize("publish"),
                                textService.Localize("speechBubbles/contentPublishedFailedByEvent"),
                                EventMessageType.Warning));
                    }
                    break;
                case PublishStatusType.FailedAwaitingRelease:
                    messages.Add(new EventMessage(
                        textService.Localize("publish"),
                        textService.Localize("publish/contentPublishedFailedAwaitingRelease",
                            new[] { string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id) }).Trim(),
                        EventMessageType.Warning));
                    break;
                case PublishStatusType.FailedHasExpired:
                    messages.Add(new EventMessage(
                        textService.Localize("publish"),
                        textService.Localize("publish/contentPublishedFailedExpired",
                            new[]
                            {
                                string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id),
                            }).Trim(),
                        EventMessageType.Warning));
                    break;
                case PublishStatusType.FailedIsTrashed:
                    //TODO: We should add proper error messaging for this!
                    break;
                case PublishStatusType.FailedContentInvalid:
                    messages.Add(new EventMessage(
                        textService.Localize("publish"),
                        textService.Localize("publish/contentPublishedFailedInvalid",
                            new[]
                            {
                                string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id),
                                string.Join(",", status.InvalidProperties.Select(x => x.Alias))
                            }).Trim(),
                        EventMessageType.Warning));
                    break;
            }

            return messages;
        }

        /// <summary>
        /// A helper method to mimic the default Umbraco notifications based on UnPublishStatus
        /// </summary>
        /// <remarks>See Umbraco source - ContentController.PostUnPublish</remarks>
        public static IEnumerable<EventMessage> GetUmbracoDefaultEventMessages(Attempt<UnPublishStatus> unPublishStatus,
            ILocalizedTextService textService)
        {
            var eventMessages = new List<EventMessage>();

            if (unPublishStatus.Success)
            {
                eventMessages.Add(new EventMessage(
                    textService.Localize("content/unPublish"),
                    textService.Localize("speechBubbles/contentUnpublished")
                ));
            }
            else if (!unPublishStatus.Success && unPublishStatus.Result.EventMessages.Count == 0)
            {
                eventMessages.Add(new EventMessage(
                    textService.Localize("speechBubbles/operationCancelledHeader"),
                    textService.Localize("speechBubbles/operationCancelledText"),
                    EventMessageType.Warning));
            }

            return eventMessages;
        }
    }
}
