using System;
using System.Web;
using System.Web.Mvc;
using umbraco.BusinessLogic;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
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
            Request.Cookies.Remove("Workflow_Preview");

            if (_previewService.Validate(nodeId, userId, taskId, guid).Result)
            {
                // auth cookie disappears somewhere in the prevew generator
                // so store it here then reapply later...
                HttpCookie umbContextCookie =
                    Request.Cookies[UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName];

                _previewService.Generate(nodeId, userId, guid);

                StateHelper.Cookies.UserContext.SetValue(umbContextCookie?.Value);
            }
            else
            {
                StateHelper.Cookies.UserContext.Clear();
                StateHelper.Cookies.Preview.Clear();

                // add a cookie to indicate that the preview request was invalid
                var cookie = new HttpCookie("Workflow_Preview", "0")
                {
                    Path = Request.Url.AbsolutePath,
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = false
                };

                Response.Cookies.Add(cookie);
            }

            return File("/app_plugins/workflow/backoffice/preview/workflow.preview.html", "text/html");
        }
    }
}
