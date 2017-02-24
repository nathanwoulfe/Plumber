using Umbraco.Core;
using Umbraco.Web.Trees;

namespace Workflow.UserGroups
{
    public class UserGroupsTreeEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            TreeControllerBase.RootNodeRendering += TreeControllerBase_RootNodeRendering;
        }

        void TreeControllerBase_RootNodeRendering(TreeControllerBase sender, TreeNodeRenderingEventArgs e)
        {
            //if (!(sender.TreeAlias == "usergroups"))
            //    return;

            //// AngularJs interceptor catches and redirects this value - haven't yet been able to add it as an Angular route in the Umbraco app.
            //e.Node.RoutePath = "/workflow/tree/groups";
        }
    }
}