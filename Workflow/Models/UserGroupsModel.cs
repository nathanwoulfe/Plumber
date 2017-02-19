using System.Collections.Generic;

namespace Workflow.Models.UserGroups
{ 

    public class UserGroupModel
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public string GroupEmail { get; set; }
        public string UsersSummary { get; set; }
        public int GroupId { get; set; }
        public List<UserGroupUserModel> Users { get; set; }
        public List<UserGroupPermissionsModel> Permissions { get; set; }

        public UserGroupModel()
        {
            Users = new List<UserGroupUserModel>();
            Permissions = new List<UserGroupPermissionsModel>();
        }
    }

    public class UserGroupPermissionsModel
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public int GroupId { get; set; }
        public int Permission { get; set; }
        public string NodeName { get; set; }
        public string Url { get; set; }
    }

    public class UserGroupUserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SectionTreeNode
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
    }
}