using System;
using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Workflow.Helpers;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Constants = Umbraco.Core.Constants;

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
                _previewService.Generate(nodeId, userId, guid);

                Utility.SetCookie(UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName,
                    HttpContext.Items[UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName] as string, $"/{nodeId}");
            }
            else
            {
                Utility.ExpireCookie(UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName);
                Utility.ExpireCookie(Constants.Web.PreviewCookieName);

                // add a cookie to indicate that the preview request was invalid
                Utility.SetCookie("Workflow_Preview", "0", httpOnly: false);
            }

            return File("/app_plugins/workflow/backoffice/preview/workflow.preview.html", "text/html");
        }
    }
}
