using Umbraco.Core;
using Umbraco.Web;

namespace UmbracoWorkflow.Actions
{
    public class WorkflowContextMenuController : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Umbraco.Web.Trees.TreeControllerBase.MenuRendering += ContentTreeController_MenuRendering;
        }

        void ContentTreeController_MenuRendering(Umbraco.Web.Trees.TreeControllerBase sender, Umbraco.Web.Trees.MenuRenderingEventArgs e)
        {
            if (sender.TreeAlias == "content" && string.Compare(e.NodeId, "-1") != 0)
            {
                var nodeName = UmbracoContext.Current.ContentCache.GetById(int.Parse(e.NodeId)).Name;
                var currentUser = UmbracoContext.Current.Security.CurrentUser.UserType;

                var i = new Umbraco.Web.Models.Trees.MenuItem("workflowHistory", "Workflow history");
                i.LaunchDialogView("/App_Plugins/Workflow/Backoffice/dialogs/workflow.history.dialog.html", "Workflow history: " + nodeName);
                i.SeperatorBefore = true;
                i.Icon = "directions-alt";

                e.Menu.Items.Insert(5, i);

                i = new Umbraco.Web.Models.Trees.MenuItem("sendForPublish", "Send for publish");
                i.LaunchDialogView("/App_Plugins/Workflow/Backoffice/dialogs/workflow.submit.dialog.html", "Send for publish approval: " + nodeName);
                i.AdditionalData.Add("isPublish", true);
                i.Icon = "check";

                e.Menu.Items.Insert(6, i);

                i = new Umbraco.Web.Models.Trees.MenuItem("sendForUnpublish", "Send for unpublish");
                i.LaunchDialogView("/App_Plugins/Workflow/Backoffice/dialogs/workflow.submit.dialog.html", "Send for unpublish approval: " + nodeName);
                i.AdditionalData.Add("isPublish", false);
                i.Icon = "delete";

                e.Menu.Items.Insert(7, i);

                if (currentUser.Alias == "admin")
                {
                    i = new Umbraco.Web.Models.Trees.MenuItem("workflowConfig", "Workflow configuration");
                    i.LaunchDialogView("/App_Plugins/Workflow/Backoffice/dialogs/workflow.config.dialog.html", "Workflow configuration: " + nodeName);
                    i.Icon = "path";

                    e.Menu.Items.Insert(8, i);
                }
            }
        }
    }    
}
