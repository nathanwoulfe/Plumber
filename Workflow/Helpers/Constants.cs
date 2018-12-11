namespace Workflow.Helpers
{
    public static class Constants
    {
        public const string Name = "Plumber";
        public const string AppSettingKey = "WorkflowInstalled";

        public const string VersionKey = "plumberVersion";
        public const string DocsKey = "plumberDocs";

        //table names
        public const string SettingsTable = "WorkflowSettings";
        public const string UserGroupsTable = "WorkflowUserGroups";
        public const string User2UserGroupTable = "WorkflowUser2UserGroup";
        public const string PermissionsTable = "WorkflowUserGroupPermissions";
        public const string InstanceTable = "WorkflowInstance";
        public const string TaskInstanceTable = "WorkflowTaskInstance";

        // github urls
        public const string LatestVersionUrl = "https://api.github.com/repos/nathanwoulfe/plumber/releases/latest";
        public const string DocsUrl = "https://api.github.com/repos/nathanwoulfe/plumber/contents/Workflow/DOCS.md";
        public const string MdMediaType = "application/vnd.github.v3.html";

        public const string ErrorGettingVersion = "Error getting version information";

        public const string GroupNameExists = "Group name already exists";
        public const string GroupCreated = "Successfully created new user group '{name}'.";
        public const string GroupDeleted = "User group has been deleted";
        public const string GroupUpdated = "User group '{name}' has been saved";
        public const string ErrorGettingGroup = "Error getting group by id {id}";
        public const string ErrorDeletingGroup = "Error deleting user group";
        public const string ErrorUpdatingGroup = "An error occurred updating the user group";

        public const string ErrorGettingPendingTasksForNode = "Error getting pending tasks for node {id}";

        public const string SettingsUpdated = "Settings updated";
        public const string SettingsNotUpdated = "Could not save settings";
        public const string ErrorGettingSettings = "Error getting settings";

        public const string HttpResponseException = "HttpResponseException";

        public const string NoNode = "Node does not exist";
        public const string NoContentType = "Content type does not exist";

        public const string PreviewUserName = "WorkflowPreview";
        public const string PreviewRouteBase = "/workflow-preview/";
        public const string PreviewFileFolder = "/app_plugins/workflow/preview";

        public const string ContentEditUrlFormat = "/umbraco#/content/content/edit/{0}";


    }
}
