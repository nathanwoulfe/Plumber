using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Workflow.Helpers;
using Workflow.Models;
using CoreConstants = Umbraco.Core.Constants;

namespace Workflow.Trees
{
    [Tree("workflow", "workflow", "Workflow")]
    [PluginController("Workflow")]
    public class WorkflowTreeController : TreeController
    {
        private const string RouteBase = "workflow/workflow/";
        private const string EditGroupRoute = "workflow/workflow/edit-group/";

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id.InvariantEquals("approvalGroups"))
            {

                var menuItem = new MenuItem
                {
                    Alias = "add",
                    Icon = "add",
                    Name = "Add group"
                };

                menuItem.LaunchDialogView("/app_plugins/workflow/backoffice/approval-groups/add.html", "Add group");

                menu.Items.Add(menuItem);
                menu.Items.Add<ActionRefresh>("Reload nodes", true);
            }
            else if (int.TryParse(id, out int _))
            {
                var menuItem = new MenuItem
                {
                    Alias = "delete",
                    Icon = "delete",
                    Name = "Delete group"
                };

                menuItem.LaunchDialogView("/app_plugins/workflow/backoffice/approval-groups/delete.html", "Delete group");

                menu.Items.Add(menuItem);
            }

            return menu;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            string rootId = CoreConstants.System.Root.ToInvariantString();

            if (id.InvariantEquals(rootId))
            {
                TreeNode groupsNode = CreateTreeNode("approvalGroups", id, queryStrings, "Approval groups", "icon-users", true,
                    $"{RouteBase}approval-groups/info");
                nodes.Add(groupsNode);

                TreeNode historyNode = CreateTreeNode("history", id, queryStrings, "History", "icon-directions-alt", false,
                    $"{RouteBase}history/info");
                nodes.Add(historyNode);

                TreeNode settingsNode = CreateTreeNode("settings", id, queryStrings, "Settings", "icon-umb-settings", false,
                    $"{RouteBase}settings/info");
                nodes.Add(settingsNode);

            }
            else if (id.InvariantEquals("approvalGroups"))
            {
                AddApprovalGroupsToTree(nodes, queryStrings);
            }

            return nodes;
        }


        /// <summary>
        /// Adds the approval group nodes to the tree.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="queryStrings">The query strings.</param>
        public void AddApprovalGroupsToTree(TreeNodeCollection nodes, FormDataCollection queryStrings)
        {
            UmbracoDatabase db = ApplicationContext.Current.DatabaseContext.Database;
            List<UserGroupPoco> userGroups = db.Fetch<UserGroupPoco>(SqlHelpers.GroupsForTree).OrderBy(x => x.Name).ToList();

            if (!userGroups.Any()) return;

            foreach (UserGroupPoco group in userGroups)
            {
                nodes.Add(CreateTreeNode(group.GroupId.ToString(), "approvalGroups", queryStrings, group.Name, "icon-users", false, $"{EditGroupRoute}{group.GroupId}"));
            }
        }
    }
}
