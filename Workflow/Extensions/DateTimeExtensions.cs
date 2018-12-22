using System;

namespace Workflow.Extensions
{
    internal static class DateTimeExtensions
    {
        private const string DateFormat = "d MMMM yyyy h:mmtt";
        private const string DateFormatNoMinute = "d MMMM yyyy htt";

        public static string ToFriendlyDate(this DateTime dateTime)
        {
            return dateTime.ToString(dateTime.Minute != 0 ? DateFormat : DateFormatNoMinute);
        }
    }
}
