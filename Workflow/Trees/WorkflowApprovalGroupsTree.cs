using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Workflow.Models;

namespace Workflow.Trees
{
    [Tree("workflow", "approval-groups", "Approval groups")]
    [PluginController("Workflow")]
    public class WorkflowApprovalGroupsTreeController : TreeController
    {
        public WorkflowApprovalGroupsTreeController()
        {
        }

        public WorkflowApprovalGroupsTreeController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
        }

        public WorkflowApprovalGroupsTreeController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper) : base(umbracoContext, umbracoHelper)
        {
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.RoutePath = "workflow/approval-groups";
            root.Icon = "icon-users";
            root.HasChildren = true;

            return root;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var db = UmbracoContext.Current.Application.DatabaseContext.Database;

            var nodes = new TreeNodeCollection();
            var treeNodes = new List<SectionTreeNode>();
            const string route = "workflow/approval-groups/edit/";

            var userGroups = db.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE deleted = 0 ORDER BY name DESC");

            if (userGroups != null && userGroups.Any())
            {
                treeNodes.AddRange(userGroups.Select(userGroup => new SectionTreeNode
                {
                    Id = userGroup.GroupId.ToString(),
                    Title = userGroup.Name, Icon = "icon-users",
                    Route = $"{route}{userGroup.GroupId}"
                }));
            }

            nodes.AddRange(treeNodes.Select(n => CreateTreeNode(n.Id, id, queryStrings, n.Title, n.Icon, false, n.Route)));

            return nodes;
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

            if (id == "-1")
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