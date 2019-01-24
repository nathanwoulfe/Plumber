using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Workflow.Events.Args;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Notifications;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Processes
{
    /// <summary>
    /// Process definition for the Document Publish workflow process.
    /// </summary>
    public class DocumentPublishProcess : WorkflowProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IContentService _contentService;
        private readonly ILocalizedTextService _textService;
        private readonly IInstancesService _instancesService;
        private readonly Emailer _emailer;
        private readonly Utility _utility;

        public static event EventHandler<InstanceEventArgs> Completed;

        public DocumentPublishProcess()
            : this(
                ApplicationContext.Current.Services.ContentService,
                ApplicationContext.Current.Services.TextService,
                new InstancesService(),
                new Emailer(),
                new Utility()
                )
        {
        }

        private DocumentPublishProcess(IContentService contentService, ILocalizedTextService textService, IInstancesService instancesService, Emailer emailer, Utility utility)
        {
            _contentService = contentService;
            _textService = textService;
            _instancesService = instancesService;
            _emailer = emailer;
            _utility = utility;

            Type = WorkflowType.Publish;
        }

        /// <summary>
        /// Performs workflow completion tasks on completion of the approval processing.
        /// </summary>
        protected override void CompleteWorkflow()
        {
            // Handle Publish At (Release At)
            if (Instance.ScheduledDate != null && Instance.ScheduledDate > DateTime.Now)
            {
                HandlePublishAt();
            }
            else // Handle Publish Now
            {
                HandlePublishNow();
            }
        }

        /// <summary>
        /// Publishes the document to the live site.
        /// </summary>
        private void HandlePublishNow()
        {
            // Have to do this prior to the publish due to workaround for "publish at" handling.
            Instance.Status = (int)WorkflowStatus.Approved;
            Instance.CompletedDate = DateTime.Now;

            // Perform the publish
            IContent node = _contentService.GetById(Instance.NodeId);

            // clear the release date
            if (node.ReleaseDate.HasValue)
            {
                node.ReleaseDate = null;
            }

            Attempt<PublishStatus> publishStatus = _contentService.PublishWithStatus(node, Instance.TaskInstances.Last().ActionedByUserId ?? _utility.GetCurrentUser().Id);

            EventMessages = GetEventMessages(publishStatus);

            if (!publishStatus.Success)
            {
                var exceptionOccured = publishStatus.Exception != null;

                var errorMessage = exceptionOccured
                    ? $" (Workflow error: {publishStatus.Exception.Message})"
                    : $" (Workflow error: Publish failed: {publishStatus.Result.StatusType.ToString()})";

                Instance.Status = (int)WorkflowStatus.Errored;
                Instance.AuthorComment += errorMessage;
                _instancesService.UpdateInstance(Instance);

                Log.Error(errorMessage);

                if (exceptionOccured)
                {
                    throw new WorkflowException(publishStatus.Exception.Message); // need eventmessages support here?
                }
                else
                {
                    throw new UmbracoOperationFailedException(publishStatus.Result.ToString());
                }
            }

            _instancesService.UpdateInstance(Instance);

            _emailer.Send(Instance, EmailType.ApprovedAndCompleted);
            Completed?.Invoke(this, new InstanceEventArgs(Instance, "PublishNow"));
        }

        /// <summary>
        /// For a document with a release date set, dont do the publish, just allow it to occur when the scheduler reaches the release date.
        /// </summary>
        private void HandlePublishAt()
        {
            try
            {
                // Just complete the workflow
                Instance.Status = (int)WorkflowStatus.Approved;
                Instance.CompletedDate = DateTime.Now;
                _instancesService.UpdateInstance(Instance);

                _emailer.Send(Instance, EmailType.ApprovedAndCompletedForScheduler);

                Completed?.Invoke(this, new InstanceEventArgs(Instance, "PublishAt"));

                // Publish will occur via scheduler.
            }
            catch (Exception ex)
            {
                string errorText = $"Error completing workflow for {Instance.Node.Name}: {ex.Message}";
                Log.Error(errorText);

                throw new WorkflowException(errorText);
            }
        }

        /// <summary>
        /// Get a collection of EventMessages that should show based on the PublishStatus
        /// This includes EventMessages added by custom Umbraco events, as well as ones
        /// Umbraco adds by default.
        /// </summary>
        private IEnumerable<EventMessage> GetEventMessages(Attempt<PublishStatus> publishStatus)
        {
            var eventMessages = new List<EventMessage>();

            eventMessages.AddRange(publishStatus.Result.EventMessages.GetAll());

            eventMessages.AddRange(NotificationHelpers.GetUmbracoDefaultEventMessages(publishStatus.Result, _textService));

            return eventMessages;
        }
    }
}

