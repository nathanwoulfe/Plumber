using System;
using Umbraco.Core.Models.Membership;
using Workflow.Helpers;

namespace Workflow.Events
{
    public class EventArgsBase : EventArgs
    {
        protected EventArgsBase()
        {
            var utility = new Utility();
            User = utility.GetCurrentUser();
        }

        private IUser User { get; set; }
    }
}
