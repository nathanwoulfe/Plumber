using System;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace Workflow.Controllers
{
    public class RouteHandler : UmbracoVirtualNodeRouteHandler
    {
        protected override IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext)
        {
            if (null == requestContext) return null;

            string path = requestContext.HttpContext.Request.Url.GetAbsolutePathDecoded();

            if (!path.StartsWith("/workflow-preview/")) return null;

            string[] segments = path.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length != 4)
            {
                return null;
            }

            IPublishedContent node = umbracoContext.ContentCache.GetById(int.Parse(segments[1]));

            return node;
        }
    }
}
