using log4net;
using System;
using Umbraco.Core;
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
        public override void CompleteWorkflow()
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
            bool success;
            var errorText = "";

            try
            {
                // Have to do this prior to the publish due to workaround for "publish at" handling.
                Instance.Status = (int)WorkflowStatus.Approved;
                Instance.CompletedDate = DateTime.Now;

                // Perform the publish
                var cs = ApplicationContext.Current.Services.ContentService;
                var node = cs.GetById(Instance.NodeId);
                success = cs.PublishWithStatus(node).Success;
            }
            catch (Exception e)
            {
                try
                {
                    // rollback the process completion.
                    Instance.Status = (int)WorkflowStatus.Errored;
                    Instance.AuthorComment += " * This workflow has errored with message " + e.Message + " *";
                }
                catch (Exception ex)
                {
                    errorText = "Unable to publish document " + Instance.Node.Name + ": " + ex.Message;
                    Log.Error(errorText, ex);
                }

                success = false;
                errorText = "Unable to publish document " + Instance.Node.Name  + ": " + e.Message;
                Log.Error(errorText, e);
            }

            ApplicationContext.Current.DatabaseContext.Database.Update(Instance);

            if (success)
            {
                Notifications.Send(Instance, EmailType.ApprovedAndCompleted);
            }
            else
            {
                throw new WorkflowException(errorText);
            }
        }

        /// <summary>
        /// For a document with a release date set, dont do the publish, just allow it to occur when the scheduler reaches the release date.
        /// </summary>
        private void HandlePublishAt()
        {
            // There is a release date set so just complete the workflow. The internal scheduler will publish this at the required time.
            var success = false;
            var errorText = "";

            try
            {
                // Just complete the workflow
                Instance.Status = (int)WorkflowStatus.Approved;
                Instance.CompletedDate = DateTime.Now;
                ApplicationContext.Current.DatabaseContext.Database.Update(Instance);
                success = true;

                // Publish will occur via scheduler.
            }
            catch (Exception ex)
            {
                errorText = "Error completing workflow for " + Instance.Node.Name + ": " + ex.Message;
                Log.Error(errorText);
            }

            if (success)
            {
                Notifications.Send(Instance, EmailType.ApprovedAndCompletedForScheduler);
            }
            else
            {
                throw new WorkflowException(errorText);
            }
        }
    }
}

