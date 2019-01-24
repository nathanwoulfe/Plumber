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
    public class DocumentUnpublishProcess : WorkflowProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static IContentService _contentService;
        private static ILocalizedTextService _textService;
        private static IInstancesService _instancesService;

        private readonly Emailer _emailer;
        private readonly Utility _utility;

        private static string _nodeName;

        public static event EventHandler<InstanceEventArgs> Completed;

        public DocumentUnpublishProcess()
            : this(
                ApplicationContext.Current.Services.ContentService,
                ApplicationContext.Current.Services.TextService,
                new InstancesService(),
                new Emailer(),
                new Utility()
            )
        {
        }

        private DocumentUnpublishProcess(IContentService contentService, ILocalizedTextService textService, IInstancesService instancesService, Emailer emailer, Utility utility)
        {
            _contentService = contentService;
            _textService = textService;
            _instancesService = instancesService;
            _emailer = emailer;
            _utility = utility;

            Type = WorkflowType.Unpublish;
        }

        /// <summary>
        /// Performs workflow completion tasks on completion of the approval processing.
        /// </summary>
        protected override void CompleteWorkflow()
        {
            _nodeName = Instance.Node.Name;

            // Handle Unpublish at (Remove At)
            if (Instance.ScheduledDate != null && Instance.ScheduledDate > DateTime.Now)
            {
                HandleUnpublishAt();
            }
            else // Handle UnPublish Now
            {
                HandleUnpublishNow();
            }
        }


        /// <summary>
        /// Removes the document from the live site.
        /// </summary>
        private void HandleUnpublishNow()
        {
            bool success;
            bool errorIsFriendly = false;
            var errorText = "";
            int workflowStatus = Instance.Status;

            try
            {
                // Have to do this prior to the publish due to workaround for "unpublish at" handling.
                Instance.Status = (int)WorkflowStatus.Approved;
                Instance.CompletedDate = DateTime.Now;
                _instancesService.UpdateInstance(Instance);

                // Perform the unpublish
                IContent node = _contentService.GetById(Instance.NodeId);

                var unPublishStatus = _contentService.WithResult().UnPublish(node, Instance.TaskInstances.Last().ActionedByUserId ?? _utility.GetCurrentUser().Id);

                EventMessages = GetEventMessages(unPublishStatus);

                success = unPublishStatus.Success;

                if (!success)
                {
                    if (unPublishStatus.Exception != null)
                    {
                        throw unPublishStatus.Exception; // hit catch block below
                    }
                    else
                    {
                        throw new UmbracoOperationFailedException(unPublishStatus.Result.ToString());
                    }

                }
            }
            catch (Exception e)
            {
                try
                {
                    // rollback the process completion.
                    Instance.Status = workflowStatus;
                    Instance.CompletedDate = null;
                    _instancesService.UpdateInstance(Instance);
                }
                catch (Exception ex)
                {
                    errorText = $"Unable to unpublish document {_nodeName}: {ex.Message}";
                    Log.Error(errorText);
                }

                success = false;
                errorText = $"Unable to unpublish document {_nodeName}: {e.Message}";
                Log.Error(errorText);
                errorIsFriendly = e is UmbracoOperationFailedException;
            }

            if (success)
            {
                _emailer.Send(Instance, EmailType.ApprovedAndCompleted);
                Log.Info("Successfully unpublished page " + Instance.Node.Name);

                Completed?.Invoke(this, new InstanceEventArgs(Instance, "UnpublishNow"));
            }
            else if (errorIsFriendly)
            {
                throw new UmbracoOperationFailedException(errorText);
            }
            else
            {
                throw new WorkflowException(errorText);
            }
        }

        /// <summary>
        /// For a document with an expiry date set, dont do the unpublish, just allow it to occur when the scheduler reaches the Expiry date.
        /// </summary>
        private void HandleUnpublishAt()
        {
            try
            {
                // Just complete the workflow
                Instance.Status = (int)WorkflowStatus.Approved;
                Instance.CompletedDate = DateTime.Now;
                _instancesService.UpdateInstance(Instance);

                _emailer.Send(Instance, EmailType.ApprovedAndCompletedForScheduler);
                // Unpublish will occur via scheduler.
                Completed?.Invoke(this, new InstanceEventArgs(Instance, "UnpublishAt"));

            }
            catch (Exception ex)
            {
                string errorText = $"Error completing workflow for {_nodeName}: {ex.Message}";
                Log.Error(errorText);

                throw new WorkflowException(errorText);
            }
        }

        /// <summary>
        /// Get a collection of EventMessages that should show based on the PublishStatus
        /// This includes EventMessages added by custom Umbraco events, as well as ones 
        /// </summary>
        /// <param name="publishStatus"></param>
        private IEnumerable<EventMessage> GetEventMessages(Attempt<UnPublishStatus> unPublishStatus)
        {
            var eventMessages = new List<EventMessage>();

            eventMessages.AddRange(unPublishStatus.Result.EventMessages.GetAll());

            eventMessages.AddRange(NotificationHelpers.GetUmbracoDefaultEventMessages(unPublishStatus, _textService));

            return eventMessages;
        }
    }
}
