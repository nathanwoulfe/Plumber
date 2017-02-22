using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

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
                treeNodes.Add(new SectionTreeNode() { Id = "groups", Title = "Groups", Icon = "icon-link", Route = string.Format("{0}{1}", route, "groups") });

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