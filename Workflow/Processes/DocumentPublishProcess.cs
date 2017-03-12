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
    public class DocumentPublishProcess : WorkflowApprovalProcess
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DocumentPublishProcess() : base()
        {
            Type = WorkflowType.Publish;
        }

        /// <summary>
        /// Performs workflow completion tasks on completion of the approval processing.
        /// </summary>
        /// <param name="userId">The user Id who performed the action which has triggered the completion of the workflow</param>
        public override void CompleteWorkflow()
        {
            // Handle Publish At (Release At)
            if (instance.ScheduledDate != null && instance.ScheduledDate > DateTime.Now)
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
            string errorText = "";

            try
            {
                // Have to do this prior to the publish due to workaround for "publish at" handling.
                instance.Status = (int)WorkflowStatus.Approved;
                instance.CompletedDate = DateTime.Now;

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

            ApplicationContext.Current.DatabaseContext.Database.Update(instance);

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
        private void HandlePublishAt()
        {
            // There is a release date set so just complete the workflow. The internal scheduler will publish this at the required time.
            bool success = false;
            string errorText = "";

            try
            {
                // Just complete the workflow
                instance.Status = (int)WorkflowStatus.Approved;
                instance.CompletedDate = DateTime.Now;
                ApplicationContext.Current.DatabaseContext.Database.Update(instance);
                success = true;

                // Publish will occur via scheduler.
            }
            catch (Exception ex)
            {
                errorText = "Error completing workflow for " + instance.Node.Name + ": " + ex.Message;
                log.Error(errorText);
            }

            if (success)
            {
                Notifications.Send(instance, EmailType.ApprovedAndCompletedForScheduler);
            }
            else
            {
                throw new WorkflowException(errorText);
            }
        }
    }
}

