using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Workflow
{
    internal static class ViewHelpers
    {
        /// <summary>
        /// Drop that cap
        /// </summary>
        public static JsonSerializerSettings CamelCase
        {
            get
            {
                return new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
            }
        }

        /// <summary>
        /// Helper for generating a response object for API errors
        /// </summary>
        /// <param name="e">The orignal exception</param>
        /// <param name="msg">Optionally return a friendly error message</param>
        /// <returns></returns>
        public static Object ApiException (Exception e, string msg = "")
        {
            return new
            {
                ExceptionType = e.GetType().Name,
                ExceptionMessage = string.IsNullOrEmpty(msg) ? e.Message : msg,
                StackTrace = e.StackTrace
            };
        }
    }
}
