using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Workflow.Helpers;
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

            if (string.IsNullOrEmpty(wsp.Email))
            {
                wsp.Email = UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress;
            }

            return wsp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetPendingTasks(int status, int count, int page)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.PendingTasks, status)
                .Skip((page - 1) * count).Take(count).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId, int count, int page)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.AllGroupTasks, groupId)
                .Skip((page - 1) * count).Take(count).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco>(SqlHelpers.AllTasksForDateRange, oldest);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<WorkflowInstancePoco> GetAllInstances()
        {
            return GetDb().Fetch<WorkflowInstancePoco, WorkflowTaskInstancePoco, UserGroupPoco, WorkflowInstancePoco>(new UserToGroupForInstanceRelator().MapIt, SqlHelpers.AllInstances);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <returns></returns>
        public List<WorkflowInstancePoco> GetAllInstancesForDateRange(DateTime oldest)
        {
            return GetDb().Fetch<WorkflowInstancePoco>(SqlHelpers.AllInstancesForDateRange, oldest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> TasksByNode(int nodeId)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksByNode, nodeId);
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public  List<WorkflowTaskInstancePoco> TasksForUser(int id, int status)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksForUser, id, status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> SubmissionsForUser(int id, int status)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.SubmissionsForUser, id, status);
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
        public  List<WorkflowTaskInstancePoco> TasksAndGroupByInstanceId(Guid guid)
        {
            return GetDb().Fetch<WorkflowTaskInstancePoco>(SqlHelpers.TasksAndGroupByInstanceId, guid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowInstancePoco> InstancesByNodeAndStatus(int node, List<int> status)
        {            
            var statusStr = string.Concat("Status = ", string.Join(" OR Status = ", status));
            return GetDb().Fetch<WorkflowInstancePoco>(string.Concat(SqlHelpers.InstanceByNodeStr, statusStr), node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<UserGroupPoco> UserGroups()
        {
            return GetDb().Fetch<UserGroupPoco, UserGroupPermissionsPoco, User2UserGroupPoco, UserGroupPoco>(new GroupsRelator().MapIt, SqlHelpers.UserGroups);            
        }

        public List<UserGroupPoco> PopulatedUserGroup(int id)
        {
            return GetDb().Fetch<UserGroupPoco, UserGroupPermissionsPoco, User2UserGroupPoco, UserGroupPoco>(new GroupsRelator().MapIt, SqlHelpers.UserGroupDetailed, id);
        }

        public List<UserGroupPoco> UserGroupsByName(string value)
        {
            return GetDb().Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE Name = @0", value);
        }

        public List<UserGroupPoco> UserGroupsByAlias(string value)
        {
            return GetDb().Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE Alias = @0", value);
        }

        public List<UserGroupPoco> UserGroupsById(int value)
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
        /// <param name="nodeId"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public List<UserGroupPermissionsPoco> PermissionsForNode(int nodeId, int? contentTypeId)
        {
            return GetDb().Fetch<UserGroupPermissionsPoco, UserGroupPoco, User2UserGroupPoco, UserGroupPermissionsPoco>(new UserToGroupForPermissionsRelator().MapIt, SqlHelpers.PermissionsByNode, nodeId, contentTypeId);
        }

        public int CountPendingTasks()
        {
            return GetDb().Fetch<int>(SqlHelpers.CountPendingTasks).First();
        }

        public int CountGroupTasks(int groupId)
        {
            return GetDb().Fetch<int>(SqlHelpers.CountGroupTasks, groupId).First();
        }

        public bool HasFlow(int nodeId)
        {
            var homepageNodeId = ApplicationContext.Current.Services.ContentService.GetById(nodeId).Path.Split(',')[1];
            return GetDb().Fetch<int>("SELECT * FROM WorkflowUserGroupPermissions WHERE NodeId = @0", homepageNodeId).Any();
        }
    }
}
