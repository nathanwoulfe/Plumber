using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workflow.Events.Args;
using Workflow.Models;

namespace Workflow.Services.Interfaces
{
    public interface IGroupService
    {
        Task<UserGroupPoco> GetPopulatedUserGroupAsync(int groupId);
        Task<UserGroupPoco> GetUserGroupAsync(int id);
        Task<UserGroupPoco> UpdateUserGroupAsync(UserGroupPoco poco);
        Task<UserGroupPoco> CreateUserGroupAsync(string name);

        Task<IEnumerable<UserGroupPoco>> GetUserGroupsAsync();

        Task DeleteUserGroupAsync(int groupId);
    }
}