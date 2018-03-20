using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Events.Args
{
    public class ConfigEventArgs : EventArgsBase
    {
        public ConfigEventArgs(Dictionary<int, List<UserGroupPermissionsPoco>> model, string type)
        {
            Model = model;
            Type = type;
        }

        private Dictionary<int, List<UserGroupPermissionsPoco>> Model { get; set; }
        private string Type { get; set; }
    }
}
