using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;

namespace Workflow.Startup
{
    public class WorkflowContextMenuController : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Umbraco.Web.Trees.TreeControllerBase.MenuRendering += ContentTreeController_MenuRendering;
        }

        private static void ContentTreeController_MenuRendering(Umbraco.Web.Trees.TreeControllerBase sender, Umbraco.Web.Trees.MenuRenderingEventArgs e)
        {
            // only add context menu to content nodes, exclude the root and recycle bin
            if (sender.TreeAlias != Constants.Trees.Content) return;

            int nodeId = Convert.ToInt32(e.NodeId);

            if (nodeId == Constants.System.Root || nodeId == Constants.System.RecycleBinContent) return;

            IContent node = ApplicationContext.Current.Services.ContentService.GetById(nodeId);

            const string dialogPath = "/App_Plugins/workflow/Backoffice/views/dialogs/";

            int menuLength = e.Menu.Items.Count;

            string nodeName = node.Name;
            IUser currentUser = UmbracoContext.Current.Security.CurrentUser;
            var items = new Umbraco.Web.Models.Trees.MenuItemList();

            var i = new Umbraco.Web.Models.Trees.MenuItem("workflowHistory", "Workflow history");
            i.LaunchDialogView(dialogPath + "workflow.history.dialog.html", "Workflow history: " + nodeName);
            i.AdditionalData.Add("width", "800px");
            i.SeperatorBefore = true;
            i.Icon = "directions-alt";

            items.Add(i);

            if (currentUser.IsAdmin())
            {
                i = new Umbraco.Web.Models.Trees.MenuItem("workflowConfig", "Workflow configuration");
                i.LaunchDialogView(dialogPath + "workflow.config.dialog.html", "Workflow configuration: " + nodeName);
                i.Icon = "path";

                items.Add(i);
            }

            if (menuLength <= 5)
            {
                e.Menu.Items.AddRange(items);
            } else
            {
                e.Menu.Items[5].SeperatorBefore = true;
                e.Menu.Items.InsertRange(5, items);
            }
        }
    }    
}
