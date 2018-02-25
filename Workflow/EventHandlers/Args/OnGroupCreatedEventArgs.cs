using System;
using Umbraco.Core.Models.Membership;
using Workflow.Models;

namespace Workflow.EventHandlers.Args
{
    public class OnGroupCreatedEventArgs : EventArgs
    {
        public UserGroupPoco Group { get; set; }
        public IUser CreatedBy { get; set; }
    }
}
