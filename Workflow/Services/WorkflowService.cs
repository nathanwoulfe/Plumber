using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Workflow.Models;

namespace Workflow.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly ILogger log;
        private readonly IPocoRepository repo;

        public WorkflowService()
            : this(
                  ApplicationContext.Current.ProfilingLogger.Logger,
                  new PocoRepository(ApplicationContext.Current.DatabaseContext.Database)
            )
        {
        }

        public WorkflowService(ILogger log, IPocoRepository repo)
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
    }
}
