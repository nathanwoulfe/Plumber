namespace Workflow.Helpers
{
    public static class MagicStrings
    {
        public const string NoNode = "Node does not exist";
        public const string Name = "Plumber";

        public const string VersionKey = "plumberVersion";
        public const string DocsKey = "plumberDocs";

        public const string LatestVersionUrl = "https://api.github.com/repos/nathanwoulfe/plumber/releases/latest";
        public const string DocsUrl = "https://api.github.com/repos/nathanwoulfe/plumber/contents/Workflow/DOCS.md";
        public const string MdMediaType = "application/vnd.github.v3.html";

        public const string GroupNameExists = "Group name already exists";
        public const string GroupCreated = "Successfully created new user group '{name}'.";
        public const string GroupDeleted = "User group has been deleted";
        public const string GroupUpdated = "User group '{name}' has been saved";
        public const string ErrorGettingGroup = "Error getting group by id {id}";

        public const string ErrorGettingPendingTasksForNode = "Error getting pending tasks for node {id}";

        public const string SettingsUpdated = "Settings updated";
        public const string SettingsNotUpdated = "Could not save settings";

        public const string HttpResponseException = "HttpResponseException";

        public const string PreviewUserName = "WorkflowPreview";
        public const string PreviewRouteBase = "/workflow-preview/";
    }
}
