using System;
using Workflow.Models;

namespace Workflow.Extensions
{
    public static class WorkflowTypeExtensions
    {
        public static string Description(this WorkflowType type, DateTime? scheduledDate)
        {
            string typeString = type.ToString().ToTitleCase();
            if (scheduledDate.HasValue)
            {
                return "Schedule for " + typeString + " at " + scheduledDate.Value.ToString("dd/MM/yy HH:mm");
            }

            return typeString;
        }

        public static string DescriptionPastTense(this WorkflowType type, DateTime? scheduledDate)
        {
            return type.Description(scheduledDate).Replace("ish", "ished").Replace("dule", "duled").Replace("for", "to be");
        }
    }
}
