using Workflow.Models;

namespace Workflow.Services.Interfaces
{
    public interface ISettingsService
    {
        WorkflowSettingsPoco GetSettings();

        void UpdateSettings(WorkflowSettingsPoco settings);
    }
}
