namespace Workflow.Helpers
{
    internal static class SqlQueries
    {
        // settings
        public const string GetSettings = @"SELECT * FROM WorkflowSettings";

        // groups
        public const string GroupsForTree = @"SELECT * FROM WorkflowUserGroups WHERE deleted = 0 ORDER BY name DESC";
        public const string UserGroups = @"SELECT * FROM WorkflowUserGroups 
                            LEFT JOIN WorkflowUserGroupPermissions
                            on WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroups.GroupId = WorkflowUser2UserGroup.GroupId
                            WHERE WorkflowUserGroups.Deleted = 0";
        public const string UserGroupDetailed = @"SELECT * FROM WorkflowUserGroups 
                            LEFT JOIN WorkflowUserGroupPermissions
                            on WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroups.GroupId = WorkflowUser2UserGroup.GroupId
                            WHERE WorkflowUserGroups.GroupId = @0";

        // instance
        public const string CountPendingInstances = @"SELECT COUNT(*) FROM WorkflowInstance WHERE Status = 3";
        public const string CountAllInstances = @"SELECT COUNT(*) FROM WorkflowInstance";

        public const string InstanceByGuid = @"SELECT * FROM WorkflowInstance WHERE Guid = @0";
        public const string InstanceByNodeStr = @"SELECT * FROM WorkflowInstance WHERE NodeId = @0";
        public const string AllInstances = @"SELECT * FROM WorkflowInstance 
                            LEFT JOIN WorkflowTaskInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            ORDER BY WorkflowInstance.CreatedDate DESC";
        public const string AllActiveInstances = @"SELECT * FROM WorkflowInstance 
                            WHERE Status IN (2, 3, 4, 7)";
        public const string AllInstancesForNode = @"SELECT * FROM WorkflowInstance 
                            LEFT JOIN WorkflowTaskInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowInstance.NodeId = @0
                            ORDER BY WorkflowInstance.CreatedDate DESC";
        public const string AllInstancesForDateRange = @"SELECT * FROM WorkflowInstance
                            WHERE CompletedDate IS NULL OR CompletedDate >= CONVERT(DATETIME, @0)";
        public const string FilteredInstancesForDateRange = @"SELECT * FROM WorkflowInstance
                            LEFT JOIN WorkflowTaskInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE (WorkflowInstance.CompletedDate IS NULL OR WorkflowInstance.CompletedDate >= CONVERT(DATETIME, @0))
                            AND (@1 = -1 OR WorkflowInstance.Status = @1)";

        // tasks
        public const string GetTask = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid   
                            WHERE WorkflowTaskInstance.Id = @0";
        public const string CountGroupTasks = @"SELECT COUNT(*) FROM WorkflowTaskInstance WHERE GroupId = @0";
        public const string CountPendingTasks = @"SELECT COUNT(*) FROM WorkflowTaskInstance WHERE Status = 3";
        public const string SubmissionsForUser = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId                     
                            WHERE WorkflowInstance.AuthorUserId = @id
                            AND WorkflowTaskInstance.Status in (@statusInts)
                            ORDER BY WorkflowTaskInstance.Status";
        public const string AllGroupTasks = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.GroupId = @0
                            ORDER BY WorkflowTaskInstance.CreatedDate DESC";
        public const string AllTasksForDateRange = @"SELECT * FROM WorkflowTaskInstance
                            WHERE (CompletedDate IS NULL OR CompletedDate >= CONVERT(DATETIME, @0))";
        public const string FilteredTasksForDateRange = @"SELECT * FROM WorkflowTaskInstance
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid                           
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE (WorkflowTaskInstance.CompletedDate IS NULL OR WorkflowTaskInstance.CompletedDate >= CONVERT(DATETIME, @0))
                            AND (@1 = -1 OR WorkflowTaskInstance.Status = @1)";
        public const string PendingTasks = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.Status in (@statusInts)";
        public const string TasksByNode = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowInstance.NodeId = @0
                            ORDER BY WorkflowTaskInstance.Id DESC";
        public const string TasksAndGroupByInstanceId = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.WorkflowInstanceGuid = @0
                            ORDER BY WorkflowTaskInstance.CreatedDate DESC";

        // permissions
        public const string PermissionsByNode = @"SELECT * FROM WorkflowUserGroupPermissions
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId           
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroupPermissions.GroupId = WorkflowUser2UserGroup.GroupId             
                            WHERE WorkflowUserGroupPermissions.NodeId = @0
                            AND WorkflowUserGroupPermissions.ContentTypeId = @1";

        public const string AllPermissionsForNode = @"SELECT * FROM WorkflowUserGroupPermissions
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId           
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroupPermissions.GroupId = WorkflowUser2UserGroup.GroupId             
                            WHERE WorkflowUserGroupPermissions.NodeId IN (@0)
                            OR WorkflowUserGroupPermissions.ContentTypeId = @1";
    }
}
