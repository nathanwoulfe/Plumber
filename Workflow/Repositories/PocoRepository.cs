using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Relators;
using Workflow.Repositories.Interfaces;

namespace Workflow.Repositories
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

        private PocoRepository(UmbracoDatabase database)
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
            List<WorkflowSettingsPoco> settings = _database.Fetch<WorkflowSettingsPoco>(SqlHelpers.GetSettings);

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
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UserGroupPermissionsPoco GetDefaultUserGroupPermissions(string name)
        {
            UserGroupPermissionsPoco permissions = _database.Fetch<UserGroupPermissionsPoco>(SqlHelpers.UserGroupBasic, name).First();
            return permissions;
        }

        /// <summary>
        /// Get all user groups and their associated permissions and user groups
        /// </summary>
        /// <returns>A list of objects of type <see cref="UserGroupPoco"/></returns>
        public IEnumerable<UserGroupPoco> GetUserGroups()
        {
            return _database.Fetch<UserGroupPoco, UserGroupPermissionsPoco, User2UserGroupPoco, UserGroupPoco>(new GroupsRelator().MapIt, SqlHelpers.UserGroups);            
        }

        /// <summary>
        /// Get a user group with its member users and permissions
        /// </summary>
        /// <param name="id">The group id</param>
        /// <returns>A list of objects of type <see cref="UserGroupPoco"/></returns>
        public UserGroupPoco GetPopulatedUserGroup(int id)
        {
            return _database
                .Fetch<UserGroupPoco, UserGroupPermissionsPoco, User2UserGroupPoco, UserGroupPoco>(
                    new GroupsRelator().MapIt,
                    SqlHelpers.UserGroupDetailed,
                    id
                ).First(g => !g.Deleted);
        }

        /// <summary>
        /// Check if the group alias is in use
        /// </summary>
        /// <param name="value">The group alias</param>
        /// <returns>bool indicating group alias existence</returns>
        public bool GroupAliasExists(string value)
        {
            return _database.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE Alias = @0", value).Any(g => !g.Deleted);
        }

        /// <summary>
        /// Check if the group name is in use
        /// </summary>
        /// <param name="value">The group name</param>
        /// <returns>bool indicating group name existence</returns>
        public bool GroupNameExists(string value)
        {
            return _database.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE Name = @0", value).Any(g => !g.Deleted);
        }

        /// <summary>
        /// Get user group by id
        /// </summary>
        /// <param name="value">The group id</param>
        /// <returns>A list of objects of type <see cref="UserGroupPoco"/></returns>
        public UserGroupPoco GetUserGroupById(int value)
        {
            return _database.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE GroupId = @0", value).FirstOrDefault();
        }

        /// <summary>
        /// Get the workflow permissions for the given node id or content type id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <param name="contentTypeId">The contentType id</param>
        /// <returns>A list of objects of type <see cref="UserGroupPermissionsPoco"/></returns>
        public List<UserGroupPermissionsPoco> PermissionsForNode(int nodeId, int? contentTypeId)
        {
            return _database.Fetch<UserGroupPermissionsPoco, UserGroupPoco, User2UserGroupPoco, UserGroupPermissionsPoco>
                (new UserToGroupForPermissionsRelator().MapIt, SqlHelpers.PermissionsByNode, nodeId, contentTypeId);
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

        public void UpdateSettings(WorkflowSettingsPoco settings)
        {
            _database.Update(settings);
        }

        /// <summary>
        /// Delete a group
        /// </summary>
        /// <param name="groupId">The id of the group to delete</param>
        public void DeleteUserGroup(int groupId)
        {
            _database.Execute("UPDATE WorkflowUserGroups SET Deleted = 1 WHERE GroupId = @0", groupId);
        }

        /// <summary>
        /// Delete all existing config for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        public void DeleteNodeConfig(int nodeId)
        {
            _database.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE NodeId = @0", nodeId);
        }

        /// <summary>
        /// Delete config for all content types
        /// </summary>
        public void DeleteContentTypeConfig()
        {
            _database.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE ContentTypeId != 0");
        }

        /// <summary>
        /// Add permission for node
        /// </summary>
        /// <param name="perm">Permission object of type <see cref="UserGroupPermissionsPoco"/></param>
        public void AddPermission(UserGroupPermissionsPoco perm)
        {
            _database.Insert(perm);
        }
    }
}
