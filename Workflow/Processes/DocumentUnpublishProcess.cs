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
    public class DocumentUnpublishProcess : WorkflowApprovalProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string _nodeName;

        public DocumentUnpublishProcess()
        {            
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
        public void HandleUnpublishNow()
        {
            bool success;
            var errorText = "";
            var originalWfStatus = Instance.WorkflowStatus;

            try
            {
                // Have to do this prior to the publish due to workaround for "unpublish at" handling.
                Instance.Status = (int)WorkflowStatus.Approved;
                Instance.CompletedDate = DateTime.Now;
                ApplicationContext.Current.DatabaseContext.Database.Update(Instance);

                // Perform the unpublish
                var cs = ApplicationContext.Current.Services.ContentService;
                var node = cs.GetById(Instance.NodeId);
                success = cs.UnPublish(node);
            }
            catch (Exception e)
            {
                try
                {
                    // rollback the process completion.
                    Instance.Status = (int)originalWfStatus;
                    Instance.CompletedDate = null;
                    ApplicationContext.Current.DatabaseContext.Database.Update(Instance);
                }
                catch (Exception ex)
                {
                    errorText = "Unable to unpublish document " + _nodeName + ": " + ex.Message;
                    Log.Error(errorText);
                }

                success = false;
                errorText = "Unable to unpublish document " + _nodeName + ": " + e.Message;
                Log.Error(errorText);
            }

            if (success)
            {
                Notifications.Send(Instance, EmailType.ApprovedAndCompleted);
                Log.Info("Successfully unpublished page " + Instance.Node.Name);
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
            // There is a remove date set so just complete the workflow. The internal scheduler will unpublish this at the required time.
            var success = false;
            var errorText = "";

            try
            {
                // Just complete the workflow
                Instance.Status = (int)WorkflowStatus.Approved;
                Instance.CompletedDate = DateTime.Now;
                ApplicationContext.Current.DatabaseContext.Database.Update(Instance);
                success = true;

                // Unpublish will occur via scheduler.
            }
            catch (Exception ex)
            {
                errorText = "Error completing workflow for " + _nodeName + ": " + ex.Message;
                Log.Error(errorText);
            }

            if (success)
            {
                Notifications.Send(Instance, EmailType.ApprovedAndCompletedForScheduler);
                var notificationText = "The document '" + _nodeName + "' has been approved for removal and is " + Instance.TypeDescriptionPastTense;

                Log.Info(notificationText);
            }
            else
            {
                throw new WorkflowException(errorText);
            }
        }
    }
}
