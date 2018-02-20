using log4net;
using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Workflow.Helpers;
using Workflow.Models;

namespace Workflow.Processes
{
    /// <summary>
    /// Process definition for the Document Publish workflow process.
    /// </summary>
    public class DocumentPublishProcess : WorkflowApprovalProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DocumentPublishProcess()
        {
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
            IContentService cs = ApplicationContext.Current.Services.ContentService;
            IContent node = cs.GetById(Instance.NodeId);

            // clear the release date
            if (node.ReleaseDate.HasValue)
            {
                node.ReleaseDate = null;
            }

            Attempt<PublishStatus> publishStatus = cs.PublishWithStatus(node);

            if (!publishStatus.Success)
            {
                Instance.Status = (int)WorkflowStatus.Errored;
                Instance.AuthorComment += $" (Workflow error: {publishStatus.Exception.Message})";

                Log.Error(publishStatus.Exception.Message);

                throw new WorkflowException(publishStatus.Exception.Message);

            }

            ApplicationContext.Current.DatabaseContext.Database.Update(Instance);
            Notifications.Send(Instance, EmailType.ApprovedAndCompleted);
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
                ApplicationContext.Current.DatabaseContext.Database.Update(Instance);

                Notifications.Send(Instance, EmailType.ApprovedAndCompletedForScheduler);

                // Publish will occur via scheduler.
            }
            catch (Exception ex)
            {
                string errorText = $"Error completing workflow for {Instance.Node.Name}: {ex.Message}";
                Log.Error(errorText);

                throw new WorkflowException(errorText);
            }
        }
    }
}

