using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Workflow.Models;

namespace Workflow.Tree
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
                const string route = "/workflow/tree/view/";
                var treeNodes = new List<SectionTreeNode>();

                var user = UmbracoContext.Current.Security.CurrentUser;

                if (user.AllowedSections.Contains("settings") || user.UserType.Alias =="admin")
                {
                    treeNodes.Add(new SectionTreeNode() { Id = "settings", Title = "Settings", Icon = "icon-umb-settings", Route = $"{route}settings"});
                    treeNodes.Add(new SectionTreeNode() { Id = "groups", Title = "Approval groups", Icon = "icon-users", Route =$"{route}groups"});
                }
                treeNodes.Add(new SectionTreeNode() { Id = "history", Title = "History", Icon = "icon-directions-alt", Route = $"{route}history"});

                nodes.AddRange(treeNodes.Select(n => CreateTreeNode(n.Id, id, queryStrings, n.Title, n.Icon, n.Id == "groups", n.Route)));

                return nodes;
            }

            if (id == "groups")
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;

                var nodes = new TreeNodeCollection();
                var treeNodes = new List<SectionTreeNode>();
                const string route = "/workflow/tree/edit/";

                var userGroups = db.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE deleted = false ORDER BY name DESC");

                if (userGroups != null && userGroups.Any())
                {
                    treeNodes.AddRange(userGroups.Select(userGroup => new SectionTreeNode() {Id = userGroup.GroupId.ToString(), Title = userGroup.Name, Icon = "icon-users", Route = $"{route}{userGroup.GroupId}"}));
                }

                nodes.AddRange(treeNodes.Select(n => CreateTreeNode(n.Id, id, queryStrings, n.Title, n.Icon, false, n.Route)));

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

            if (id == "groups")
            {
                menu.Items.Add(new MenuItem()
                {
                    Alias = "add",
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
}