using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence;
using Workflow.Models;
using Workflow.Relators;

namespace Workflow
{
    class PocoRepository
    {
        private Database db;
        public PocoRepository(Database database)
        {
            db = database;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> TasksByStatus(int status)
        {
            return db.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksByStatus, status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<User2UserGroupPoco> GroupsForUserById(int id)
        {
            return db.Fetch<User2UserGroupPoco>(SqlHelpers.GroupsForUserById, id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksWithGroup()
        {
            return db.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksWithGroup);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksByUserAndStatus(int id, int status)
        {
            return db.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksByUserAndStatus, id, status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  WorkflowInstancePoco InstanceByTaskId(int id)
        {
            return db.Fetch<WorkflowInstancePoco>(SqlHelpers.InstanceByTaskId, id).First();
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksByInstanceId(Guid guid)
        {
            return db.Fetch<WorkflowTaskInstancePoco>(SqlHelpers.TasksByInstanceId, guid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksAndGroupByInstanceId(Guid guid)
        {
            return db.Fetch<WorkflowTaskInstancePoco>(SqlHelpers.TasksByInstanceId, guid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  List<User2UserGroupPoco> UsersByGroupId(int id)
        {
            return db.Fetch<User2UserGroupPoco>(SqlHelpers.UsersByGroupId, id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public  List<WorkflowInstancePoco> InstancesByNodeAndStatus(int node, List<int> status)
        {            
            var statusStr = string.Join(" OR ", status);
            return db.Fetch<WorkflowInstancePoco>(string.Concat(SqlHelpers.InstanceByNodeStr, statusStr), node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  List<UserGroupPoco> UserGroups()
        {
            return db.Fetch<UserGroupPoco, User2UserGroupPoco, UserGroupPoco>(new UserToGroupRelator().MapIt, SqlHelpers.UserGroups);
        }

        public  List<UserGroupPoco> UserGroupsByProperty(string property, string value)
        {
            return db.Fetch<UserGroupPoco>(SqlHelpers.UserGroupByProperty, property, value);
        }

        public  UserGroupPoco NewestGroup()
        {
            return db.Fetch<UserGroupPoco>(SqlHelpers.NewestGroup).First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  List<UserGroupPermissionsPoco> PermissionsForGroup(int id)
        {
            return db.Fetch<UserGroupPermissionsPoco>(SqlHelpers.PermissionsForGroup, id);
        }

    }
}
