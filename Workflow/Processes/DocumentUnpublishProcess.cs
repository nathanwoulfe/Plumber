using log4net;
using System;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Models;

namespace Workflow
{
    /// <summary>
    /// Process definition for the Document Publish workflow process.
    /// </summary>
    public class DocumentUnpublishProcess : WorkflowApprovalProcess
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string NodeName;

        public DocumentUnpublishProcess(): base()
        {            
            this.Type = WorkflowType.Unpublish;
        }

        /// <summary>
        /// Performs workflow completion tasks on completion of the approval processing.
        /// </summary>
        /// <param name="userId">The user Id who performed the action which has triggered the completion of the workflow</param>
        public override void CompleteWorkflow(int userId)
        {
            NodeName = instance.Node.Name;

            // Handle Unpublish at (Remove At)
            if (this.instance.ScheduledDate != null && this.instance.ScheduledDate > DateTime.Now)
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
            bool success = false;
            string errorText = "";
            WorkflowStatus originalWFStatus = instance._Status;

            try
            {
                // Have to do this prior to the publish due to workaround for "unpublish at" handling.
                instance.Status = (int)WorkflowStatus.Approved;
                instance.CompletedDate = DateTime.Now;
                ApplicationContext.Current.DatabaseContext.Database.Update(instance);

                // Perform the unpublish
                var cs = ApplicationContext.Current.Services.ContentService;
                var node = cs.GetById(instance.NodeId);
                success = cs.UnPublish(node);
            }
            catch (Exception e)
            {
                try
                {
                    // rollback the process completion.
                    instance.Status = (int)originalWFStatus;
                    instance.CompletedDate = null;
                    ApplicationContext.Current.DatabaseContext.Database.Update(instance);
                }
                catch (Exception ex)
                {
                    errorText = "Unable to unpublish document " + NodeName + ": " + ex.Message;
                    log.Error(errorText);
                }

                success = false;
                errorText = "Unable to unpublish document " + NodeName + ": " + e.Message;
                log.Error(errorText);
            }

            if (success)
            {
                Notifications.Send(instance, EmailType.ApprovedAndCompleted);
                log.Info("Successfully unpublished page " + this.instance.Node.Name);
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
            bool success = false;
            string errorText = "";

            try
            {
                // Just complete the workflow
                instance.Status = (int)WorkflowStatus.Approved;
                instance.CompletedDate = DateTime.Now;
                ApplicationContext.Current.DatabaseContext.Database.Update(instance);
                success = true;

                // Unpublish will occur via scheduler.
            }
            catch (Exception ex)
            {
                errorText = "Error completing workflow for " + NodeName + ": " + ex.Message;
                log.Error(errorText);
            }

            if (success)
            {
                Notifications.Send(instance, EmailType.ApprovedAndCompletedForScheduler);
                string notificationText = "The document '" + NodeName + "' has been approved for removal and is " + this.instance.TypeDescriptionPastTense;

                log.Info(notificationText);
            }
            else
            {
                throw new WorkflowException(errorText);
            }
        }
    }
}
