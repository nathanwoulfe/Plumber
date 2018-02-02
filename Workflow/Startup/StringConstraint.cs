using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Web.Http.Routing;

namespace Workflow.Startup
{
    public class NonZeroConstraint : IHttpRouteConstraint
    {
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
            IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (!values.TryGetValue(parameterName, out object value) || value == null) return false;

            long longValue;
            if (value is long l)
            {
                longValue = l;
                return longValue != 0;
            }

            string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (long.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out longValue))
            {
                return longValue != 0;
            }
            return false;
        }
    }
}
