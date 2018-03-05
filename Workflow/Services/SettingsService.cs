using Umbraco.Core;
using Umbraco.Core.Logging;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services.Interfaces;

namespace Workflow.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ILogger _log;
        private readonly IPocoRepository _repo;

        public SettingsService()
            : this(
                ApplicationContext.Current.ProfilingLogger.Logger,
                new PocoRepository(ApplicationContext.Current.DatabaseContext.Database)
            )
        {
        }

        private SettingsService(ILogger log, IPocoRepository repo)
        {
            _log = log;
            _repo = repo;
        }

        /// <summary>
        /// Get the workflow settings
        /// </summary>
        /// <returns></returns>
        public WorkflowSettingsPoco GetSettings()
        {
            WorkflowSettingsPoco settings = _repo.GetSettings();

            return settings;
        }

        /// <summary>
        /// Update the workflow settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void UpdateSettings(WorkflowSettingsPoco settings)
        {
            _repo.UpdateSettings(settings);
        }
    }
}
