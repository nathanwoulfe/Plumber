using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Models;
using Workflow.Relators;

namespace Workflow
{
    class PocoRepository
    {
        /// ensure GetDb() connection exists
        private Database GetDb()
        {
            return ApplicationContext.Current.DatabaseContext.Database;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> TasksByStatus(int status)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksByStatus, status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<User2UserGroupPoco> GroupsForUserById(int id)
        {
            return GetDb().Fetch<User2UserGroupPoco>(SqlHelpers.GroupsForUserById, id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksWithGroup()
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksWithGroup);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksByUserAndStatus(int id, int status)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksByUserAndStatus, id, status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  WorkflowInstancePoco InstanceByTaskId(int id)
        {
            return GetDb().Fetch<WorkflowInstancePoco>(SqlHelpers.InstanceByTaskId, id).First();
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksByInstanceId(Guid guid)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco>(SqlHelpers.TasksByInstanceId, guid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksAndGroupByInstanceId(Guid guid)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco>(SqlHelpers.TasksByInstanceId, guid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  List<User2UserGroupPoco> UsersByGroupId(int id)
        {
            return GetDb().Fetch<User2UserGroupPoco>(SqlHelpers.UsersByGroupId, id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public  List<WorkflowInstancePoco> InstancesByNodeAndStatus(int node, List<int> status)
        {            
            var statusStr = string.Concat("Status = ", string.Join(" OR Status = ", status));
            return GetDb().Fetch<WorkflowInstancePoco>(string.Concat(SqlHelpers.InstanceByNodeStr, statusStr), node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  List<UserGroupPoco> UserGroups()
        {
            return GetDb().Fetch<UserGroupPoco, User2UserGroupPoco, UserGroupPoco>(new UserToGroupRelator().MapIt, SqlHelpers.UserGroups);
        }

        public List<UserGroupPoco> AllUserGroups()
        {
            return GetDb().Fetch<UserGroupPoco>(SqlHelpers.UserGroups);
        }

        public List<UserGroupPoco> UserGroupsByProperty(string property, string value)
        {
            return GetDb().Fetch<UserGroupPoco>(SqlHelpers.UserGroupByProperty, property, value);
        }

        public List<UserGroupPoco> UserGroupsByName(string value)
        {
            return GetDb().Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE Name = @0", value);
        }

        public List<UserGroupPoco> UserGroupsByAlias(string value)
        {
            return GetDb().Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE Alias = @0", value);
        }

        public List<UserGroupPoco> UserGroupsById(string value)
        {
            return GetDb().Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE GroupId = @0", value);
        }

        public  UserGroupPoco NewestGroup()
        {
            return GetDb().Fetch<UserGroupPoco>(SqlHelpers.NewestGroup).First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  List<UserGroupPermissionsPoco> PermissionsForGroup(int id)
        {
            return GetDb().Fetch<UserGroupPermissionsPoco>(SqlHelpers.PermissionsForGroup, id);
        }

    }
}
