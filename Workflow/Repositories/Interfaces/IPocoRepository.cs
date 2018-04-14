using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Repositories.Interfaces
{
    public interface IPocoRepository
    {
        bool HasFlow(int nodeId);
        bool GroupAliasExists(string value);
        bool GroupNameExists(string value);

        WorkflowSettingsPoco GetSettings();

        UserGroupPoco InsertUserGroup(string name, string alias, bool deleted);
        UserGroupPoco GetPopulatedUserGroup(int id);
        UserGroupPoco GetUserGroupById(int id);

        UserGroupPermissionsPoco GetDefaultUserGroupPermissions(int id);

        IEnumerable<UserGroupPoco> GetUserGroups();

        List<UserGroupPermissionsPoco> PermissionsForNode(int nodeId, int contentTypeId = 0);
        List<UserGroupPermissionsPoco> GetAllPermissions();

        void AddUserToGroup(User2UserGroupPoco user);
        void AddPermission(UserGroupPermissionsPoco perm);

        void DeleteUserGroup(int groupId);
        void DeleteNodeConfig(int nodeId);
        void DeleteContentTypeConfig();
        void DeleteUsersFromGroup(int groupId);

        void UpdateUserGroup(UserGroupPoco poco);
        void UpdateSettings(WorkflowSettingsPoco settings);
    }
}