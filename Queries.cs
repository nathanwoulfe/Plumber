namespace Workflow
{
    class SqlHelpers
    {
        public const string UserGroupById = @"SELECT * FROM WorkflowUser2UserGroup WHERE UserId = @0";

        public const string UserGroupWithUsersById =@"SELECT * FROM WorkflowUserGroups LEFT OUTER JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroups.GroupId = WorkflowUser2UserGroup.GroupId
                            WHERE WorkflowUserGroups.GroupId = @0";

        public const string Tasks = @"SELECT * FROM WorkflowTaskInstance LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId";

        public const string TasksByAuthorAndStatus = @"SELECT * FROM WorkflowTaskInstance LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId                            
                            WHERE WorkflowInstance.AuthorUserId = @0
                            AND WorkflowTaskInstance.Status = @1";

        public const string GroupsWithUsersByStatus = @"SELECT * FROM WorkflowTaskInstance LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.Status = @0";

        public const string InstanceByTaskId = @"SELECT * FROM WorkflowInstance WHERE Id = @0";

        public const string TaskByInstanceId = @"SELECT * FROM WorkflowTaskInstance WHERE WorkflowInstanceGuid = @0";

        public const string TaskAndGroupByInstanceId = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowInstanceGuid = @0";

        public const string UsersByGroupId = @"SELECT * FROM WorkflowUser2UserGroup WHERE GroupId = @0";

        public const string PermissionsByNodeAndType = @"SELECT * FROM WorkflowUserGroupPermissions WHERE NodeId = @0 AND Permission = @1";
    }
}
