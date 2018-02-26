using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow
{
    public interface IPocoRepository
    {
        int CountGroupTasks(int groupId);
        int CountPendingTasks();
        List<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId, int count, int page);
        List<WorkflowInstancePoco> GetAllInstances();
        List<WorkflowInstancePoco> GetAllInstancesForDateRange(DateTime oldest);
        List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest);
        List<WorkflowTaskInstancePoco> GetPendingTasks(IEnumerable<int> status, int count, int page);
        WorkflowSettingsPoco GetSettings();
        bool HasFlow(int nodeId);
        WorkflowInstancePoco InstanceByGuid(Guid guid);
        List<WorkflowInstancePoco> InstancesByNodeAndStatus(int node, List<int> status = null);
        UserGroupPoco NewestGroup();
        List<UserGroupPermissionsPoco> PermissionsForNode(int nodeId, int? contentTypeId);
        List<UserGroupPoco> PopulatedUserGroup(int id);
        List<WorkflowTaskInstancePoco> SubmissionsForUser(int id, IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> TasksAndGroupByInstanceId(Guid guid);
        List<WorkflowTaskInstancePoco> TasksByNode(int nodeId);
        List<WorkflowTaskInstancePoco> TasksForUser(int id, int status);
        List<UserGroupPoco> UserGroups();
        List<UserGroupPoco> UserGroupsByAlias(string value);
        List<UserGroupPoco> UserGroupsById(int value);
        List<UserGroupPoco> UserGroupsByName(string value);
        UserGroupPoco InsertUserGroup(string name, string alias, bool deleted);
        void DeleteUsersFromGroup(int groupId);
        void AddUserToGroup(User2UserGroupPoco user);
        void UpdateUserGroup(UserGroupPoco poco);
        void DeleteUserGroup(int groupId);
    }
}