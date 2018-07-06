using System;

namespace Workflow.Extensions
{
    internal static class DateTimeExtensions
    {
        private const string DateFormat = "d MMM yyyy \"at\" h:mmtt";
        private const string DateFormatNoMinute = "d MMM yyyy \"at\" htt";

        public static string ToFriendlyDate(this DateTime dateTime)
        {
            bool hasMinutes = dateTime.Minute != 0;
            return dateTime.ToString(hasMinutes ? DateFormat : DateFormatNoMinute);
        }
    }
}
