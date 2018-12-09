using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Workflow.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string value)
        {
            if (value == null)
                return null;

            value = char.ToUpper(value[0]) + value.Substring(1);

            return Regex.Replace(value, "([A-Z]+?(?=(([A-Z]?[a-z])|$))|[0-9]+)", " $1").Trim();
        }

        public static bool IsValidEmailAddress(this string value)
        {
            try
            {
                var m = new MailAddress(value);
                return m.Address == value;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
