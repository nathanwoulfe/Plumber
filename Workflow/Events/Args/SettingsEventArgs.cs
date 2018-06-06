using Workflow.Models;

namespace Workflow.Events.Args
{
    public class SettingsEventArgs : EventArgsBase
    {
        public SettingsEventArgs(WorkflowSettingsPoco settings)
        {
            Settings = settings;
        }

        public WorkflowSettingsPoco Settings { get; set; }
    }
}
