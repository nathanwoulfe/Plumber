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
            object value;
            if (!values.TryGetValue(parameterName, out value) || value == null) return false;

            long longValue;
            if (value is long)
            {
                longValue = (long)value;
                return longValue != 0;
            }

            var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (long.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out longValue))
            {
                return longValue != 0;
            }
            return false;
        }
    }
}
