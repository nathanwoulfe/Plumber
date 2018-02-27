using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Repositories
{
    public interface IPocoRepository
    {
        int CountGroupTasks(int groupId);
        int CountPendingTasks();

        bool HasFlow(int nodeId);

        List<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId, int count, int page);
        List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest);
        List<WorkflowTaskInstancePoco> GetPendingTasks(IEnumerable<int> status, int count, int page);
        List<WorkflowTaskInstancePoco> SubmissionsForUser(int id, IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> TasksAndGroupByInstanceId(Guid guid);
        List<WorkflowTaskInstancePoco> TasksByNode(int nodeId);
        List<WorkflowTaskInstancePoco> TasksForUser(int id, int status);

        List<WorkflowInstancePoco> GetAllInstances();
        List<WorkflowInstancePoco> GetAllInstancesForDateRange(DateTime oldest);
        List<WorkflowInstancePoco> InstancesByNodeAndStatus(int node, List<int> status = null);

        WorkflowSettingsPoco GetSettings();

        WorkflowInstancePoco InstanceByGuid(Guid guid);

        UserGroupPoco NewestGroup();
        UserGroupPoco InsertUserGroup(string name, string alias, bool deleted);

        List<UserGroupPoco> PopulatedUserGroup(int id);
        List<UserGroupPoco> UserGroups();
        List<UserGroupPoco> UserGroupsByAlias(string value);
        List<UserGroupPoco> UserGroupsById(int value);
        List<UserGroupPoco> UserGroupsByName(string value);

        List<UserGroupPermissionsPoco> PermissionsForNode(int nodeId, int? contentTypeId);

        void DeleteUsersFromGroup(int groupId);
        void AddUserToGroup(User2UserGroupPoco user);
        void UpdateUserGroup(UserGroupPoco poco);
        void DeleteUserGroup(int groupId);
        void DeleteNodeConfig(int nodeId);
        void DeleteContentTypeConfig();
        void AddPermissionForContentType(UserGroupPermissionsPoco perm);
        void AddPermissionForNode(UserGroupPermissionsPoco perm);
    }
}