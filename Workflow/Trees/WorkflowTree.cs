using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using umbraco.providers.members;
using Umbraco.Core;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Workflow.Models;
using CoreConstants = Umbraco.Core.Constants;

namespace Workflow.Trees
{
    [Tree("workflow", "workflow", "Workflow")]
    [PluginController("Workflow")]
    public class WorkflowTreeController : TreeController
    {
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();
            var rootId = CoreConstants.System.Root.ToInvariantString();

            if (id.InvariantEquals(rootId))
            {
                //
            } else if (id.InvariantEquals("3"))
            {
                var menuItem = new MenuItem
                {
                    Alias = "add",
                    Icon = "add",
                    Name = "Add group"
                };

                menuItem.LaunchDialogView("/app_plugins/workflow/backoffice/approval-groups/add.html", "Add group");

                menu.Items.Add(menuItem);
                menu.Items.Add<RefreshNode, umbraco.BusinessLogic.Actions.ActionRefresh>("Reload nodes", true);
            }

            return menu;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            var rootId = CoreConstants.System.Root.ToInvariantString();

            if (id.InvariantEquals(rootId))
            {
                var groupsNode = CreateTreeNode("3", id, queryStrings, "Approval groups", "icon-users", false,
                    "workflow/workflow/approval-groups/info");
                nodes.Add(groupsNode);

                var historyNode = CreateTreeNode("1", id, queryStrings, "History", "icon-directions-alt", false,
                    "workflow/workflow/history/info");
                nodes.Add(historyNode);

                var settingsNode = CreateTreeNode("2", id, queryStrings, "Settings", "icon-umb-settings", false,
                    "workflow/workflow/settings/info");
                nodes.Add(settingsNode);

            }
            else if (id.InvariantEquals("3"))
            {
                AddApprovalGroupsToTree(nodes, queryStrings);
            }

            return nodes;
        }



        /// <summary>
        /// Adds a layout node to the tree.
        /// </summary>
        /// <param name="nodes">
        /// The node collection to add the layout to.
        /// </param>
        /// <param name="queryStrings">The query strings.</param>
        public void AddApprovalGroupsToTree(TreeNodeCollection nodes, FormDataCollection queryStrings)
        {
            //var formatUrl = "/workflow/groups/edit/{0}";
            //var layoutId = GuidHelper.GetString(layout.Id);
            //var layoutRoute = string.Format(formatUrl, layoutId);
            //var layoutName = layout.Name.Fallback("Unnamed");
            //var parentId = layout.Path[layout.Path.Length - 2];
            //var strParentId = GuidHelper.GetString(parentId);
            //var layoutNode = Tree.CreateTreeNode(layoutId,
            //    strParentId, queryStrings, layoutName,
            //    LayoutsConstants.ItemIcon, false, layoutRoute);
            //nodes.Add(layoutNode);
        }
    }
}
