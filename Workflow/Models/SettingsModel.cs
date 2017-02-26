using System.Collections.Generic;

namespace Workflow.Models
{
    public class SettingsModel
    {
        public string DefaultApprover { get; set; }
        public List<string> FastTrack { get; set; }
        public string Email { get; set; }
        public string EditUrl { get; set; }
        public string SiteUrl { get; set; }

        public SettingsModel()
        {
            FastTrack = new List<string>();
        }
    }
}
