using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Workflow.Models;

namespace Workflow.Services
{
    public class GroupService : IGroupService
    {
        private readonly ILogger log;
        private readonly IPocoRepository repo;

        public GroupService()
            : this(
                  ApplicationContext.Current.ProfilingLogger.Logger,
                  new PocoRepository(ApplicationContext.Current.DatabaseContext.Database)
            )
        {
        }

        public GroupService(ILogger log, IPocoRepository repo)
        {
            this.log = log;
            this.repo = repo;
        }

        public Task<UserGroupPoco> CreateUserGroupAsync(string name)
        {
            var existing = repo.UserGroupsByName(name).Any();

            if (existing)
                return null;

            return Task.FromResult(repo.InsertUserGroup(name, name.Replace(" ", "-"), false));
        }

        public Task<UserGroupPoco> GetUserGroupAsync(int id)
        {
            List<UserGroupPoco> result = repo.PopulatedUserGroup(id);

            return Task.FromResult(result.FirstOrDefault(r => !r.Deleted));
        }

        public Task<IEnumerable<UserGroupPoco>> GetUserGroupsAsync()
        {
            List<UserGroupPoco> result = repo.UserGroups();

            return Task.FromResult(result.Where(r => !r.Deleted));
        }

        public Task<UserGroupPoco> UpdateUserGroupAsync(UserGroupPoco poco)
        {
            var nameExists = repo.UserGroupsByName(poco.Name).Any();
            var existingPoco = repo.UserGroupsById(poco.GroupId).First();

            if (poco.Name != existingPoco.Name && nameExists)
                return Task.FromResult((UserGroupPoco)null);

            repo.DeleteUsersFromGroup(poco.GroupId);

            foreach (var user in existingPoco.Users)
                repo.AddUserToGroup(user);

            repo.UpdateUserGroup(poco);

            return Task.FromResult(poco);
        }

        public Task DeleteUserGroupAsync(int groupId)
        {
            return Task.Run(() => repo.DeleteUserGroup(groupId));
        }
    }
}
