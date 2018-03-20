using log4net;
using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Workflow.Events.Args;
using Workflow.Helpers;
using Workflow.Models;
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
        private static IInstancesService _instancesService;
        private static string _nodeName;

        public static event EventHandler<InstanceEventArgs> Completed;

        public DocumentUnpublishProcess()
            : this(
                ApplicationContext.Current.Services.ContentService,
                new InstancesService()
            )
        {
        }

        private DocumentUnpublishProcess(IContentService contentService, IInstancesService instancesService)
        {
            _contentService = contentService;
            _instancesService = instancesService;

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
                success = _contentService.UnPublish(node, Instance.TaskInstances.Last().ActionedByUserId ?? Utility.GetCurrentUser().Id);
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
            }

            if (success)
            {
                Notifications.Send(Instance, EmailType.ApprovedAndCompleted);
                Log.Info("Successfully unpublished page " + Instance.Node.Name);

                Completed?.Invoke(this, new InstanceEventArgs(Instance, "UnpublishNow"));
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

                Notifications.Send(Instance, EmailType.ApprovedAndCompletedForScheduler);
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
    }
}
