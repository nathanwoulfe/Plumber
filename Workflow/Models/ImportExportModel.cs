using System.Collections.Generic;

namespace Workflow.Models
{
    public class ImportExportModel
    {
        public WorkflowSettingsExport Settings { get; set; }
        public IEnumerable<UserGroupExport> UserGroups { get; set; }
        public IEnumerable<User2UserGroupExport> User2UserGroup { get; set; }
        public IEnumerable<UserGroupPermissionsExport> UserGroupPermissions { get; set; }
    }

    /// <summary>
    /// A simple representation of the settings object for export/import actions
    /// </summary>
    public class WorkflowSettingsExport
    {
        public string DefaultApprover { get; set; }
        public string Email { get; set; }
        public string EditUrl { get; set; }
        public string SiteUrl { get; set; }
        public int FlowType { get; set; }
        public bool SendNotifications { get; set; }
        public string ExcludeNodes { get; set; }
    }

    /// <summary>
    /// A simple representation of the user2usergroup object for export/import actions
    /// </summary>
    public class User2UserGroupExport
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
    }

    /// <summary>
    /// A simple representation of the user group object for export/import actions
    /// </summary>
    public class UserGroupExport
    {
        public int GroupId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string GroupEmail { get; set; }
        public bool Deleted { get; set; }
    }

    /// <summary>
    /// A simple representation of the user group permissions object for export/import actions
    /// </summary>
    public class UserGroupPermissionsExport
    {
        public int GroupId { get; set; }
        public int NodeId { get; set; }
        public int ContentTypeId { get; set; }
        public int Permission { get; set; }
        public string Condition { get; set; }
    }
}
