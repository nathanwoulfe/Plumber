namespace Workflow
{
    public class SqlHelpers
    {        
        // groups
        public const string UserGroupBasic = @"SELECT * FROM WorkFlowUserGroups WHERE GroupId = @0";
        public const string NewestGroup = @"SELECT TOP 1 * FROM WorkflowUserGroups ORDER BY GroupId DESC";
        public const string UserGroups = @"SELECT * FROM WorkflowUserGroups 
                            LEFT JOIN WorkflowUserGroupPermissions
                            on WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroups.GroupId = WorkflowUser2UserGroup.GroupId";
        public const string UserGroupDetailed = @"SELECT * FROM WorkflowUserGroups 
                            LEFT JOIN WorkflowUserGroupPermissions
                            on WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroups.GroupId = WorkflowUser2UserGroup.GroupId
                            WHERE WorkflowUserGroups.GroupId = @0";

        // instance
        public const string InstanceByTaskId = @"SELECT * FROM WorkflowInstance WHERE Id = @0";
        public const string InstanceByNodeStr = @"SELECT * FROM WorkflowInstance WHERE NodeId = @0 AND ";
        public const string AllInstances = @"SELECT * FROM WorkflowInstance 
                            LEFT JOIN WorkflowTaskInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId";
        public const string AllInstancesForDateRange = @"SELECT * FROM WorkflowInstance
                            WHERE CreatedDate >= CONVERT(DATETIME, @0)";

        // tasks
        public const string CountGroupTasks = @"SELECT COUNT(*) FROM WorkflowTaskInstance WHERE GroupId = @0";
        public const string CountPendingTasks = @"SELECT COUNT(*) FROM WorkflowTaskInstance WHERE Status = 3";
        public const string TasksWithGroup = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowTaskInstance.GroupId = WorkflowUser2UserGroup.GroupId";
        public const string TasksForUser = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowTaskInstance.GroupId = WorkflowUser2UserGroup.GroupId                         
                            WHERE WorkflowUser2UserGroup.UserId = @0
                            AND WorkflowTaskInstance.Status = @1";
        public const string SubmissionsForUser = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId                     
                            WHERE WorkflowInstance.AuthorUserId = @0
                            AND WorkflowTaskInstance.Status = @1";
        public const string AllGroupTasks = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.GroupId = @0
                            ORDER BY WorkflowTaskInstance.CreatedDate
                            OFFSET @1 ROWS FETCH NEXT @2 ROWS ONLY";
        public const string AllTasksForDateRange = @"SELECT * FROM WorkflowTaskInstance
                            WHERE CreatedDate >= CONVERT(DATETIME, @0)";
        public const string PendingTasks = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.Status = @0
                            ORDER BY WorkflowTaskInstance.CreatedDate
                            OFFSET @1 ROWS FETCH NEXT @2 ROWS ONLY";
        public const string TasksByNode = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowInstance.NodeId = @0";                                                                
        public const string TasksAndGroupByInstanceId = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowInstanceGuid = @0";

        // permissions
        public const string PermissionsByNode = @"SELECT * FROM WorkflowUserGroupPermissions
                            LEFT JOIN WorkflowUserGroups
                            ON WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId           
                            LEFT JOIN WorkflowUser2UserGroup
                            on WorkflowUserGroupPermissions.GroupId = WorkflowUser2UserGroup.GroupId             
                            WHERE WorkflowUserGroupPermissions.NodeId = @0
                            AND WorkflowUserGroupPermissions.ContentTypeId = @1";
    }
}
