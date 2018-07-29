using Microsoft.Owin;
using Owin;
using Umbraco.Web;
using Workflow;

//To use this startup class, change the appSetting value in the web.config called 
// "owin:appStartup" to be "WorkflowOwinStartup"
[assembly: OwinStartup("WorkflowOwinStartup", typeof(WorkflowOwinStartup))]

namespace Workflow
{
    public class WorkflowOwinStartup : UmbracoDefaultOwinStartup
    {
        protected override void ConfigureMiddleware(IAppBuilder app)
        {
            base.ConfigureMiddleware(app);

            app.Use<WorkflowAuthenticationMiddleware>();
        }
    }
}
