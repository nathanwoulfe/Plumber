using Umbraco.Core;

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
                var i = new Umbraco.Web.Models.Trees.MenuItem("workflowConfig", "Workflow configuration");
                i.AdditionalData.Add("actionView", "/App_Plugins/Workflow/Backoffice/dialogs/workflow.config.dialog.html");
                i.Icon = "directions-alt";

                e.Menu.Items.Insert(5, i);

                var ii = new Umbraco.Web.Models.Trees.MenuItem("sendForPublish", "Send for publish");
                ii.AdditionalData.Add("actionView", "/App_Plugins/Workflow/Backoffice/dialogs/workflow.publish.dialog.html");
                ii.Icon = "check";

                e.Menu.Items.Insert(6, ii);

                var iii = new Umbraco.Web.Models.Trees.MenuItem("sendForUnpublish", "Send for unpublish");
                iii.AdditionalData.Add("actionView", "/App_Plugins/Workflow/Backoffice/Workflow/dialogs/workflow.unpublish.dialog.html");
                iii.Icon = "delete";

                e.Menu.Items.Insert(7, iii);
            }
        }
    }    
}
