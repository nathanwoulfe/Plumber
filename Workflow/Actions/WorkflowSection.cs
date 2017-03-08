using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Workflow.Models;

namespace Workflow.Actions
{
    [Tree("workflow", "tree", "Workflow")]
    [PluginController("Workflow")]
    public class WorkflowTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            if (id == Constants.System.Root.ToInvariantString())
            {

                var nodes = new TreeNodeCollection();
                var route = "/workflow/tree/view/";
                var treeNodes = new List<SectionTreeNode>();

                treeNodes.Add(new SectionTreeNode() { Id = "settings", Title = "Settings", Icon = "icon-link", Route = string.Format("{0}{1}", route, "settings") });
                treeNodes.Add(new SectionTreeNode() { Id = "history", Title = "History", Icon = "icon-link", Route = string.Format("{0}{1}", route, "history") });
                treeNodes.Add(new SectionTreeNode() { Id = "groups", Title = "Approval groups", Icon = "icon-link", Route = string.Format("{0}{1}", route, "groups") });

                foreach (var n in treeNodes)
                {
                    var nodeToAdd = CreateTreeNode(n.Id, id, queryStrings, n.Title, n.Icon, n.Id == "groups" ? true : false, n.Route);
                    nodes.Add(nodeToAdd);
                }

                return nodes;
            } else if (id == "usergroups")
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;

                var nodes = new TreeNodeCollection();
                var treeNodes = new List<SectionTreeNode>();
                var route = "/workflow/tree/edit/";

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
                    var nodeToAdd = CreateTreeNode(n.Id, id, queryStrings, n.Title, n.Icon, false, n.Route);
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
            int result;

            if (id == "usergroups")
            {
                menu.Items.Add(new MenuItem()
                {
                    Alias = "addGroup",
                    Name = "Create",
                    Icon = "add"
                });
                menu.Items.Add<RefreshNode, umbraco.BusinessLogic.Actions.ActionRefresh>("Reload nodes", true);
            }
            else if (int.TryParse(id, out result))
            {
                menu.Items.Add<umbraco.BusinessLogic.Actions.ActionDelete>("Delete group");
            }

            return menu;
        }
    }

    public class SectionTreeNode
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
    }
}