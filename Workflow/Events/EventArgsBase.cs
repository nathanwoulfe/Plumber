using System;
using Umbraco.Core.Models.Membership;
using Workflow.Helpers;

namespace Workflow.Events
{
    public class EventArgsBase : EventArgs
    {
        protected EventArgsBase()
        {
            User = Utility.GetCurrentUser();
        }

        private IUser User { get; set; }
    }
}
