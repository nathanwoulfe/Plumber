using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Workflow.Helpers
{
    internal static class ViewHelpers
    {
        /// <summary>
        /// Drop that cap
        /// </summary>
        public static JsonSerializerSettings CamelCase => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Helper for generating a response object for API errors
        /// </summary>
        /// <param name="e">The orignal exception</param>
        /// <param name="msg">Optionally return a friendly error message</param>
        /// <returns></returns>
        public static object ApiException (Exception e, string msg = "")
        {
            return new
            {
                ExceptionType = e.GetType().Name,
                ExceptionMessage = string.IsNullOrEmpty(msg) ? e.Message : msg,
                e.StackTrace
            };
        }
    }
}
