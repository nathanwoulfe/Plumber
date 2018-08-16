using System;
using System.Linq;

namespace Workflow.Tests
{
    public class Utility
    {
        private static readonly Random Random = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public const int CurrentUserId = 666;

        public static int RandomInt()
        {
            return Random.Next();
        }

        /// <summary>
        /// Helper for creating group names
        /// </summary>
        /// <returns></returns>
        public static string RandomString()
        {
            return new string(Enumerable.Repeat(Chars, 8)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static object ObjectValue(object T, string propName)
        {
            return T.GetType().GetProperty(propName)?.GetValue(T, null);
        }
    }
}
