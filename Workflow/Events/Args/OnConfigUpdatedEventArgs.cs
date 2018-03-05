using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Workflow.Models;

namespace Workflow.Events.Args
{
    public class OnConfigUpdatedEventArgs : EventArgs
    {
        public Dictionary<int, List<UserGroupPermissionsPoco>> Model { get; set; }
        public IUser UpdatedBy { get; set; }
    }
}
