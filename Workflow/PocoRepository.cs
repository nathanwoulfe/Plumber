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
        /// <returns></returns>
        public WorkflowSettingsPoco GetSettings()
        {
            var wsp = new WorkflowSettingsPoco();
            var db = GetDb();
            var settings = db.Fetch<WorkflowSettingsPoco>("SELECT * FROM WorkflowSettings");

            if (settings.Any())
            {
                wsp = settings.First();
            }
            else
            {
                db.Insert(wsp);
            }

            return wsp;
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
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> TasksByNode(string nodeId)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksByNode, nodeId);
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
            return GetDb().Fetch<UserGroupPoco, User2UserGroupPoco, UserGroupPoco>(new UsersToGroupsRelator().MapIt, SqlHelpers.UserGroups);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public List<UserGroupPermissionsPoco> PermissionsForNode(int nodeId, int? contentTypeId)
        {
            // TODO: Get all this in one request - permissions with groups and users
            var perms = GetDb().Fetch<UserGroupPermissionsPoco, UserGroupPoco>(SqlHelpers.PermissionsByNode, nodeId, contentTypeId);
            if (perms.Any())
            {
                foreach (var p in perms)
                {
                    p.UserGroup.Users = UsersByGroupId(p.GroupId);
                }
            }
            return perms;
        }
    }
}
