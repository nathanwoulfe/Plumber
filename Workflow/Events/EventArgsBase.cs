using System;
using Umbraco.Core.Models.Membership;
using Workflow.Helpers;

namespace Workflow.Events
{
    public class EventArgsBase : EventArgs
    {
        private readonly Utility _utility;

        protected EventArgsBase()
        {
            _utility = new Utility();
            User = _utility.GetCurrentUser() ?? null;
        }

        private IUser User { get; set; }
    }
}
