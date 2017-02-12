using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;

namespace UmbracoWorkflow.Actions
{
    public class UserPermissionsTreeController : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Umbraco.Web.Trees.ContentTreeController.MenuRendering += ContentTreeController_MenuRendering;
        }

        void ContentTreeController_MenuRendering(Umbraco.Web.Trees.TreeControllerBase sender, Umbraco.Web.Trees.MenuRenderingEventArgs e)
        {
            if (sender.TreeAlias == "content" && string.Compare(e.NodeId, "-1") != 0)
            {
                var i = new Umbraco.Web.Models.Trees.MenuItem("workflowConfig", "Workflow configuration");
                i.AdditionalData.Add("actionView", "/App_Plugins/Workflow/Backoffice/UserPermissions/view.html");
                i.Icon = "directions-alt";

                e.Menu.Items.Insert(5, i);

                var ii = new Umbraco.Web.Models.Trees.MenuItem("sendForPublish", "Send for publish");
                ii.AdditionalData.Add("actionView", "/App_Plugins/Workflow/Backoffice/Workflow/dialogs/publishDialog.html");
                ii.Icon = "check";

                e.Menu.Items.Insert(6, ii);

                var iii = new Umbraco.Web.Models.Trees.MenuItem("sendForUnpublish", "Send for unpublish");
                iii.AdditionalData.Add("actionView", "/App_Plugins/Workflow/Backoffice/Workflow/dialogs/unpublishDialog.html");
                iii.Icon = "delete";

                e.Menu.Items.Insert(7, iii);
            }
        }
    }    
}
