using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Workflow.Models.UserGroups;
using Workflow.Models;

namespace Workflow.UserGroups
{
    [Tree("users", "usergroups", "User groups")]
    [PluginController("Workflow")]
    public class UserGroupsTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            if (id == Constants.System.Root.ToInvariantString())
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;

                var nodes = new TreeNodeCollection();
                var treeNodes = new List<SectionTreeNode>();
                var route = "/users/usergroups/edit/";

                var userGroups = db.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups ORDER BY name DESC");

                if (userGroups != null && userGroups.Any())
                {
                    foreach (var userGroup in userGroups)
                    {
                        treeNodes.Add(new SectionTreeNode() { Id = userGroup.GroupId.ToString(), Title = userGroup.Name, Icon = "icon-users", Route = string.Format("{0}{1}", route, userGroup.GroupId) });
                    }
                }

                foreach (var n in treeNodes)
                {
                    var nodeToAdd = CreateTreeNode(n.Id, null, queryStrings, n.Title, n.Icon, false, n.Route);
                    nodes.Add(nodeToAdd);
                }

                return nodes;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Get menu/s for nodes in tree
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                menu.Items.Add(new MenuItem()
                {
                    Alias = "add",
                    Name = "Create",
                    Icon = "add"
                });
                menu.Items.Add<RefreshNode, ActionRefresh>("Reload nodes", true);
            }
            else
            {
                menu.Items.Add<ActionDelete>("Delete group");
            }

            return menu;
        }
    }
}