using System.Collections.Generic;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services
{
    public interface IGroupService
    {
        Task<UserGroupPoco> GetUserGroupAsync(int id);
        Task<IEnumerable<UserGroupPoco>> GetUserGroupsAsync();
        Task<UserGroupPoco> CreateUserGroupAsync(string name);
    }
}