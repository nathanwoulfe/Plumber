using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Workflow.EventHandlers.Args;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Repositories;

namespace Workflow.Services
{
    /// <summary>
    /// Service for persisting changes to workflow config on nodes or content types4
    /// </summary>
    public class ConfigService : IConfigService
    {
        private readonly ILogger log;
        private readonly IPocoRepository repo;

        public static event EventHandler ConfigUpdated;
        protected virtual void OnConfigUpdated(EventArgs e)
        {
            ConfigUpdated?.Invoke(this, e);
        }

        public ConfigService()
            : this(
                  ApplicationContext.Current.ProfilingLogger.Logger,
                  new PocoRepository(ApplicationContext.Current.DatabaseContext.Database)
            )
        {
        }

        public ConfigService(ILogger log, IPocoRepository repo)
        {
            this.log = log;
            this.repo = repo;
        }

        /// <summary>
        /// Update the stored workflow config
        /// </summary>
        /// <param name="model">Dictionary representing the indexed permissions for the node</param>
        /// <returns>Bool representing success state</returns>
        public Task<bool> UpdateNodeConfigAsync(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            if (null == model || !model.Any()) return Task.FromResult(false);

            KeyValuePair<int, List<UserGroupPermissionsPoco>> permission = model.First();

            repo.DeleteNodeConfig(permission.Key);

            if (!permission.Value.Any()) return Task.FromResult(false);

            foreach (UserGroupPermissionsPoco poco in permission.Value)
                repo.AddPermissionForNode(poco);

            // emit event
            OnConfigUpdated(new OnConfigUpdatedEventArgs
            {
                Model = model,
                UpdatedBy = Utility.GetCurrentUser()
            });

            return Task.FromResult(true);
        }

        /// <summary>
        /// Update the stored workflow config for all content types
        /// </summary>
        /// <param name="model">Dictionary representing the indexed permissions for the content type</param>
        /// <returns>Bool representing success state</returns>
        public Task<bool> UpdateContentTypeConfigAsync(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            if (null == model || !model.Any()) return Task.FromResult(false);

            repo.DeleteContentTypeConfig();           
                
            foreach (KeyValuePair<int, List<UserGroupPermissionsPoco>> permission in model)
            {
                if (!permission.Value.Any()) continue;
                foreach (UserGroupPermissionsPoco perm in permission.Value)
                {
                    repo.AddPermissionForContentType(perm);
                }
            }

            // emit event
            OnConfigUpdated(new OnConfigUpdatedEventArgs
            {
                Model = model,
                UpdatedBy = Utility.GetCurrentUser()
            });

            return Task.FromResult(true);
        }
    }
}
