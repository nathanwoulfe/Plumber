using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Models;

namespace Workflow
{
    /// <summary>
    /// Process definition for the Document Publish workflow process.
    /// </summary>
    public class DocumentPublishProcess : TwoStepApprovalProcess
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DocumentPublishProcess(Database db) : base(db)
        {
            Type = WorkflowType.Publish;
        }

        /// <summary>
        /// Performs workflow completion tasks on completion of the approval processing.
        /// </summary>
        /// <param name="userId">The user Id who performed the action which has triggered the completion of the workflow</param>
        public override void CompleteWorkflow(int userId)
        {
            // Handle Publish At (Release At)
            if (instance.ScheduledDate != null && instance.ScheduledDate > DateTime.Now)
            {
               // HandlePublishAt(userId);
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
            string errorText = "";

            try
            {
                // Have to do this prior to the publish due to workaround for "publish at" handling.
                instance.Status = (int)WorkflowStatus.Completed;
                instance.CompletedDate = DateTime.Now;
                db.Update(instance);

                // Perform the publish
                var cs = ApplicationContext.Current.Services.ContentService;
                var node = cs.GetById(instance.NodeId);
                success = cs.PublishWithStatus(node).Success;
            }
            catch (Exception e)
            {
                try
                {
                    // rollback the process completion.
                    instance.Status = (int)WorkflowStatus.Errored;
                    instance.AuthorComment += " * This workflow has errored with message " + e.Message + " *";
                }
                catch (Exception ex)
                {
                    errorText = "Unable to publish document " + instance.Node.Name + ": " + ex.Message;
                    log.Error(errorText, ex);
                }

                success = false;
                errorText = "Unable to publish document " + instance.Node.Name  + ": " + e.Message;
                log.Error(errorText, e);
            }

            db.Update(instance);

            if (success)
            {
                Notifications.Send(instance, EmailType.ApprovedAndCompleted);
            }
            else
            {
                throw new WorkflowException(errorText);
            }
        }

        /// <summary>
        /// For a document with a release date set, dont do the publish, just allow it to occur when the scheduler reaches the release date.
        /// </summary>
        /// <param name="userId"></param>
        private void HandlePublishAt(int userId)
        {
            // There is a release date set so just complete the workflow. The internal scheduler will publish this at the required time.
            bool success = false;
            string errorText = "";

            //try
            //{
            //    // Just complete the workflow
            //    instance.Status = WorkflowStatus.Completed;
            //    instance.CompletedDate = DateTime.Now;
            //    dbContext.SaveChanges();
            //    success = true;

            //    // Publish will occur via scheduler.
            //}
            //catch (Exception ex)
            //{
            //    errorText = "Error completing workflow for " + instance.Document.Text + ": " + ex.Message;
            //    log.Error(errorText);
            //}

            //if (success)
            //{
            //    SendNotification(EmailType.ApprovedAndCompletedForScheduler);
            //    string notificationText = "The document '" + instance.Document.Text + "' has been approved and is " + instance.TypeDescriptionPastTense;

            //    BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.save, "Release Scheduled", notificationText);
            //    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Custom, instance.NodeId, notificationText);
            //    log.Info(notificationText);
            //}
            //else
            //{
            //    BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.warning, "Release Schedule Failed", "The workflow failed with error " + errorText);
            //    throw new WorkflowException(errorText);
            //}
        }
    }
}

