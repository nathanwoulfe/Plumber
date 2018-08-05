using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Workflow.Helpers;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Controllers
{
    public class OfflinePreviewController : RenderMvcController
    {
        private readonly IPreviewService _previewService;

        public OfflinePreviewController()
        {
            _previewService = new PreviewService();
        }

        public ActionResult Index(RenderModel model, int nodeId, int userId, int taskId, Guid guid)
        {
            Utility.ExpireCookie("Workflow_Preview");

            if (_previewService.Validate(nodeId, userId, taskId, guid).Result)
            {
                // auth cookie disappears somewhere in the prevew generator
                // so store it here then reapply later...
                HttpCookie umbContextCookie =
                    Request.Cookies[UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName];

                _previewService.Generate(nodeId, userId, guid);

                Utility.SetCookie(UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName,
                    umbContextCookie?.Value);
            }
            else
            {
                Utility.ExpireCookie(UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName);
                Utility.ExpireCookie(Constants.Web.PreviewCookieName);

                // add a cookie to indicate that the preview request was invalid
                Utility.SetCookie("Workflow_Preview", "0", false);
            }

            return File("/app_plugins/workflow/backoffice/preview/workflow.preview.html", "text/html");
        }
    }
}
