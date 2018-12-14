using System;
using Workflow.Events.Args;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services.Interfaces;

namespace Workflow.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IPocoRepository _repo;

        public static event EventHandler<SettingsEventArgs> Updated;

        public SettingsService() : this(new PocoRepository())
        {
        }

        private SettingsService(IPocoRepository repo)
        {
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
            Updated?.Invoke(this, new SettingsEventArgs(settings));
        }
    }
}
