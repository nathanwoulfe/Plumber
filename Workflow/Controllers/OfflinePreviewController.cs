using System;
using System.Web;
using System.Web.Mvc;
using umbraco.BusinessLogic;
using umbraco.presentation.preview;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Workflow.Services;
using Workflow.Services.Interfaces;
using User = umbraco.BusinessLogic.User;

namespace Workflow.Controllers
{
    public class OfflinePreviewController : RenderMvcController
    {
        private readonly IPreviewService _previewService;

        public OfflinePreviewController()
        {
            _previewService = new PreviewService();
        }

        public ActionResult Index(RenderModel model, int nodeId, int userId, string guid)
        {
            var user = new User(userId);
            var realGuid = new Guid(guid);

            HttpCookie umbContextCookie =
                UmbracoContext.Current.HttpContext.Request.Cookies[
                    UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName];

            // for some reasons the auth cookie disappears...
            var pc = new PreviewContent(user, realGuid, false)
            {
                XmlContent = _previewService.Fetch(realGuid)
            };
            
            pc.ActivatePreviewCookie();
            
            // TODO -> refactor
            StateHelper.Cookies.UserContext.SetValue(umbContextCookie?.Value);

            return new FilePathResult(IOHelper.MapPath("/app_plugins/workflow/backoffice/preview/workflow.preview.html"), "text/html");
        }
    }
}
