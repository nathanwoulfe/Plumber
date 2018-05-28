namespace Workflow.Helpers
{
    public class SqlHelpers
    {        
        // settings
        public const string GetSettings = @"SELECT * FROM WorkflowSettings";

        // groups
        public const string UserGroupBasic = @"SELECT * FROM WorkFlowUserGroups WHERE GroupId = @0";
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
        public const string AllInstancesForDateRange = @"SELECT * FROM WorkflowInstance
                            WHERE CompletedDate IS NULL OR CompletedDate >= CONVERT(DATETIME, @0)";

        // tasks
        public const string CountGroupTasks = @"SELECT COUNT(*) FROM WorkflowTaskInstance WHERE GroupId = @0";
        public const string CountPendingTasks = @"SELECT COUNT(*) FROM WorkflowTaskInstance WHERE Status = 3";
        //public const string TasksWithGroup = @"SELECT * FROM WorkflowTaskInstance 
        //                    LEFT JOIN WorkflowInstance
        //                    on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
        //                    LEFT JOIN WorkflowUserGroups
        //                    on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
        //                    LEFT JOIN WorkflowUser2UserGroup
        //                    on WorkflowTaskInstance.GroupId = WorkflowUser2UserGroup.GroupId";
        //public const string TasksForUser = @"SELECT * FROM WorkflowTaskInstance 
        //                    LEFT JOIN WorkflowInstance
        //                    on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
        //                    LEFT JOIN WorkflowUserGroups
        //                    on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
        //                    LEFT JOIN WorkflowUser2UserGroup
        //                    on WorkflowTaskInstance.GroupId = WorkflowUser2UserGroup.GroupId                         
        //                    WHERE WorkflowUser2UserGroup.UserId = @0
        //                    AND WorkflowTaskInstance.Status = @1";
        public const string SubmissionsForUser = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId                     
                            WHERE WorkflowInstance.AuthorUserId = @id
                            AND WorkflowTaskInstance.Status in (@statusInts)
                            ORDER BY WorkflowTaskInstance.CreatedDate DESC";
        public const string AllGroupTasks = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.GroupId = @0
                            ORDER BY WorkflowTaskInstance.CreatedDate DESC";
        public const string AllTasksForDateRange = @"SELECT * FROM WorkflowTaskInstance
                            WHERE (CompletedDate IS NULL) OR (CompletedDate >= CONVERT(DATETIME, @0))";
        public const string PendingTasks = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowTaskInstance.Status in (@statusInts)
                            ORDER BY WorkflowTaskInstance.CreatedDate DESC";
        public const string TasksByNode = @"SELECT * FROM WorkflowTaskInstance 
                            LEFT JOIN WorkflowInstance
                            on WorkflowTaskInstance.WorkflowInstanceGuid = WorkflowInstance.Guid
                            LEFT JOIN WorkflowUserGroups
                            on WorkflowTaskInstance.GroupId = WorkflowUserGroups.GroupId
                            WHERE WorkflowInstance.NodeId = @0
                            ORDER BY WorkflowTaskInstance.CreatedDate DESC";                                                                
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
    }
}
