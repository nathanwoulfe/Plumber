namespace Workflow
{
    class SqlHelpers
    {
        // users
        public const string UsersByGroupId = @"SELECT * FROM WorkflowUser2UserGroup WHERE GroupId = @0";
        
        // groups
        public const string GroupsForUserById = @"SELECT * FROM WorkflowUser2UserGroup WHERE UserId = @0";
        public const string UserGroupById = @"SELECT * FROM WorkFlowUserGroups WHERE GroupId = @0";
        public const string UserGroupByProperty = @"SELECT * FROM WorkflowUserGroups WHERE @0 = @1";
        public const string NewestGroup = @"SELECT TOP 1 * FROM WorkflowUserGroups ORDER BY GroupId DESC";
        public const string UserGroups = @"SELECT * FROM WorkflowUserGroups 
                            LEFT JOIN WorkflowUserGroupPermissions
                            on WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroups.GroupId = WorkflowUser2UserGroup.GroupId";
        public const string UserGroupWithUsersById = @"SELECT * FROM WorkflowUserGroups 
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroups.GroupId = WorkflowUser2UserGroup.GroupId
                            WHERE WorkflowUserGroups.GroupId = @0";

        // instance
        public const string InstanceByTaskId = @"SELECT * FROM WorkflowInstance WHERE Id = @0";
        public const string InstanceByNodeStr = @"SELECT * FROM WorkflowInstance WHERE NodeId = @0 AND ";

        // tasks
        public const string TasksWithGroup = @"SELECT * FROM WorkflowTaskInstance LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId";
        public const string TasksByUserAndStatus = @"SELECT * FROM WorkflowTaskInstance LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId                            
                            WHERE WorkflowInstance.AuthorUserId = @0
                            AND WorkflowTaskInstance.Status = @1";
        public const string TasksByStatus = @"SELECT * FROM WorkflowTaskInstance LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.Status = @0";
        public const string TasksByNode = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            ON WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowUserGroups.GroupId = WorkflowTaskInstance.GroupId
                            WHERE WorkflowInstance.NodeId = @0";                                                                
        public const string TasksByInstanceId = @"SELECT * FROM WorkflowTaskInstance WHERE WorkflowInstanceGuid = @0";
        public const string TasksAndGroupByInstanceId = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowInstanceGuid = @0";

        // permissions
        public const string PermissionsByNodeAndType = @"SELECT * FROM WorkflowUserGroupPermissions
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId                            
                            WHERE WorkflowUserGroupPermissions.NodeId = @0 AND WorkflowUserGroupPermissions.Permission = @1";
        public const string PermissionsByNode = @"SELECT * FROM WorkflowUserGroupPermissions
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId           
                            WHERE WorkflowUserGroupPermissions.NodeId = @0
                            OR WorkflowUserGroupPermissions.ContentTypeId = @1";
        public const string PermissionsForGroup = @"SELECT * FROM WorkflowUserGroupPermissions WHERE GroupId = @0";
    }
}
