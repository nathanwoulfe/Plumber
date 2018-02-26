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
    /// <summary>
    /// The class responsible for all interactions with the workflow tables in the Umbraco database
    /// </summary>
    public class PocoRepository : IPocoRepository
    {
        private readonly UmbracoDatabase _database;

        public PocoRepository()
            : this(ApplicationContext.Current.DatabaseContext.Database)
        {
        }

        public PocoRepository(UmbracoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Get the current workflow settings, or persist an empty instance if none exist
        /// </summary>
        /// <returns>A object of type <see cref="WorkflowSettingsPoco"/> representing the current settings</returns>
        public WorkflowSettingsPoco GetSettings()
        {
            var wsp = new WorkflowSettingsPoco();
            List<WorkflowSettingsPoco> settings = _database.Fetch<WorkflowSettingsPoco>("SELECT * FROM WorkflowSettings");

            if (settings.Any())
            {
                wsp = settings.First();
            }
            else
            {
                _database.Insert(wsp);
            }

            if (string.IsNullOrEmpty(wsp.Email))
            {
                wsp.Email = UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress;
            }

            return wsp;
        }

        /// <summary>
        /// Get pending workflow tasks matching any of the provided status values
        /// </summary>
        /// <param name="status">A collection of WorkflowStatus integers</param>
        /// <param name="count">Number of items to return</param>
        /// <param name="page">Index of the page to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetPendingTasks(IEnumerable<int> status, int count, int page)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.PendingTasks, new { statusInts = status.Select(s => s.ToString()).ToArray() })
                .Skip((page - 1) * count).Take(count).ToList();
        }

        /// <summary>
        /// Get all pending workflow tasks matching any of the provided status values
        /// </summary>
        /// <param name="status">A collection of WorkflowStatus integers</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.PendingTasks, new { statusInts = status.Select(s => s.ToString()).ToArray() }).ToList();
        }

        /// <summary>
        /// Get all tasks for the given group id
        /// </summary>
        /// <param name="groupId">Id of group to query</param>
        /// <param name="count">Number of items to return</param>
        /// <param name="page">Index of the page to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId, int count, int page)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.AllGroupTasks, groupId)
                .Skip((page - 1) * count).Take(count).ToList();
        }

        /// <summary>
        /// Get all tasks created after the given date
        /// </summary>
        /// <param name="oldest">The creation date of the oldest tasks to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest)
        {
            return _database.Fetch<WorkflowTaskInstancePoco>(SqlHelpers.AllTasksForDateRange, oldest);
        }


        /// <summary>
        /// Get all workflow instances
        /// </summary>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public List<WorkflowInstancePoco> GetAllInstances()
        {
            return _database.Fetch<WorkflowInstancePoco, WorkflowTaskInstancePoco, UserGroupPoco, WorkflowInstancePoco>(new UserToGroupForInstanceRelator().MapIt, SqlHelpers.AllInstances);
        }

        /// <summary>
        /// Get all workflow instances created after the given date
        /// </summary>
        /// <param name="oldest">The creation date of the oldest instances to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public List<WorkflowInstancePoco> GetAllInstancesForDateRange(DateTime oldest)
        {
            return _database.Fetch<WorkflowInstancePoco>(SqlHelpers.AllInstancesForDateRange, oldest);
        }

        /// <summary>
        /// Get all tasks for the given node 
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> TasksByNode(int nodeId)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksByNode, nodeId);
        }

        /// <summary>
        /// Get all tasks for the given user
        /// </summary>
        /// <param name="id">The user id</param>
        /// <param name="status">The task status</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> TasksForUser(int id, int status)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksForUser, id, status);
        }

        /// <summary>
        /// Get all tasks created by the given user
        /// </summary>
        /// <param name="id">The user id</param>
        /// <param name="status">The task status collection</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> SubmissionsForUser(int id, IEnumerable<int> status)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.SubmissionsForUser, new { id, statusInts = status.Select(s => s.ToString()).ToArray() });
        }

        /// <summary>
        /// Get a single instance by guid
        /// </summary>
        /// <param name="guid">The instance guid</param>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public WorkflowInstancePoco InstanceByGuid(Guid guid)
        {
            return _database.Fetch<WorkflowInstancePoco>(SqlHelpers.InstanceByGuid, guid).First();
        }

        /// <summary>
        /// Get tasks and associated group by instance guid
        /// </summary>
        /// <param name="guid">The instance guid</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> TasksAndGroupByInstanceId(Guid guid)
        {
            return _database.Fetch<WorkflowTaskInstancePoco>(SqlHelpers.TasksAndGroupByInstanceId, guid);
        }

        /// <summary>
        /// Get all instances matching the given status[es] for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <param name="status">Optional list of WorkflowStatus integers. If not provided, method returns all instances for the node.</param>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public List<WorkflowInstancePoco> InstancesByNodeAndStatus(int nodeId, List<int> status = null)
        {
            if (status == null || !status.Any())
                return _database.Fetch<WorkflowInstancePoco>(SqlHelpers.InstanceByNodeStr, nodeId);


            string statusStr = string.Concat("Status = ", string.Join(" OR Status = ", status));
            if (!string.IsNullOrEmpty(statusStr))
            {
                statusStr = " AND " + statusStr;
            }

            return _database.Fetch<WorkflowInstancePoco>(string.Concat(SqlHelpers.InstanceByNodeStr, statusStr), nodeId);
        }

        /// <summary>
        /// Get all user groups and their associated permissions and user groups
        /// </summary>
        /// <returns>A list of objects of type <see cref="UserGroupPoco"/></returns>
        public List<UserGroupPoco> UserGroups()
        {
            return _database.Fetch<UserGroupPoco, UserGroupPermissionsPoco, User2UserGroupPoco, UserGroupPoco>(new GroupsRelator().MapIt, SqlHelpers.UserGroups);            
        }

        /// <summary>
        /// Get a user group with its member users and permissions
        /// </summary>
        /// <param name="id">The group id</param>
        /// <returns>A list of objects of type <see cref="UserGroupPoco"/></returns>
        public List<UserGroupPoco> PopulatedUserGroup(int id)
        {
            return _database
                .Fetch<UserGroupPoco, UserGroupPermissionsPoco, User2UserGroupPoco, UserGroupPoco>(
                    new GroupsRelator().MapIt,
                    SqlHelpers.UserGroupDetailed,
                    id
                );
        }

        /// <summary>
        /// Get user group by name
        /// </summary>
        /// <param name="value">The group name</param>
        /// <returns>A list of objects of type <see cref="UserGroupPoco"/></returns>
        public List<UserGroupPoco> UserGroupsByName(string value)
        {
            return _database.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE Name = @0", value);
        }

        /// <summary>
        /// Get user group by alias
        /// </summary>
        /// <param name="value">The group alias</param>
        /// <returns>A list of objects of type <see cref="UserGroupPoco"/></returns>
        public List<UserGroupPoco> UserGroupsByAlias(string value)
        {
            return _database.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE Alias = @0", value);
        }

        /// <summary>
        /// Get user group by id
        /// </summary>
        /// <param name="value">The group id</param>
        /// <returns>A list of objects of type <see cref="UserGroupPoco"/></returns>
        public List<UserGroupPoco> UserGroupsById(int value)
        {
            return _database.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE GroupId = @0", value);
        }

        /// <summary>
        /// Get the most-recently created user group
        /// </summary>
        /// <returns>An object of type <see cref="UserGroupPoco"/></returns>
        public UserGroupPoco NewestGroup()
        {
            return _database.Fetch<UserGroupPoco>(SqlHelpers.NewestGroup).First();
        }

        /// <summary>
        /// Get the workflow permissions for the given node id or content type id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <param name="contentTypeId">The contentType id</param>
        /// <returns>A list of objects of type <see cref="UserGroupPermissionsPoco"/></returns>
        public List<UserGroupPermissionsPoco> PermissionsForNode(int nodeId, int? contentTypeId)
        {
            return _database.Fetch<UserGroupPermissionsPoco, UserGroupPoco, User2UserGroupPoco, UserGroupPermissionsPoco>(new UserToGroupForPermissionsRelator().MapIt, SqlHelpers.PermissionsByNode, nodeId, contentTypeId);
        }

        /// <summary>
        /// Get a count of all pending tasks
        /// </summary>
        /// <returns>An integer representing the number of pending workflow tasks</returns>
        public int CountPendingTasks()
        {
            return _database.Fetch<int>(SqlHelpers.CountPendingTasks).First();
        }

        /// <summary>
        /// Get a count of all tasks assigned to the given group id
        /// </summary>
        /// <param name="groupId">The group id</param>
        /// <returns>An integer representing the number of pending workflow tasks assigned to the group</returns>
        public int CountGroupTasks(int groupId)
        {
            return _database.Fetch<int>(SqlHelpers.CountGroupTasks, groupId).First();
        }

        /// <summary>
        /// Check that the given node has a workflow assigned - this is checked on the homepage node, as all workflows will ultimately inherit from the homepage
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>A boolean reflecting the workflow state on the homepage node</returns>
        public bool HasFlow(int nodeId)
        {
            string homepageNodeId = ApplicationContext.Current.Services.ContentService.GetById(nodeId).Path.Split(',')[1];
            return _database.Fetch<int>("SELECT * FROM WorkflowUserGroupPermissions WHERE NodeId = @0", homepageNodeId).Any();
        }

        /// <summary>
        /// Inserts a new usergroup into the database
        /// </summary>
        /// <param name="name">The group name</param>
        /// <param name="alias">The group alias</param>
        /// <param name="deleted">The group state</param>
        /// <returns>The newly created user group, of type <see cref="UserGroupPoco"/></returns>
        public UserGroupPoco InsertUserGroup(string name, string alias, bool deleted)
        {
            var poco = new UserGroupPoco
            {
                Name = name,
                Alias = alias,
                Deleted = deleted
            };

            _database.Save(poco);
            return poco;
        }

        /// <summary>
        /// Removes all users from the given group
        /// </summary>
        /// <param name="groupId">The group id</param>
        public void DeleteUsersFromGroup(int groupId)
        {
            _database.Execute("DELETE FROM WorkflowUser2UserGroup WHERE GroupId = @0", groupId);
        }

        /// <summary>
        /// Adds the given user record to the database, creating a relationship with a group
        /// </summary>
        /// <param name="user">The user to insert, of type <see cref="User2UserGroupPoco"/></param>
        public void AddUserToGroup(User2UserGroupPoco user)
        {
            _database.Insert(user);
        }

        /// <summary>
        /// Persist changes to a usergroup
        /// </summary>
        /// <param name="poco">The group to update, of type <see cref="UserGroupPoco"/></param>
        public void UpdateUserGroup(UserGroupPoco poco)
        {
            _database.Update(poco);
        }

        /// <summary>
        /// Delete a group
        /// </summary>
        /// <param name="groupId">The id of the group to delete</param>
        public void DeleteUserGroup(int groupId)
        {
            _database.Execute("UPDATE WorkflowUserGroups SET Deleted = 1 WHERE GroupId = @0", groupId);
        }
    }
}
