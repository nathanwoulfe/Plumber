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

        public  Dictionary<int, List<UserGroupPermissionsPoco>> Model { get; set; }
        public string Type { get; set; }
    }
}
