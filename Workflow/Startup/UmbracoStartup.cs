using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;
using Workflow.Helpers;
using Constants = Workflow.Helpers.Constants;
using Installer = Workflow.Helpers.Installer;

namespace Workflow.Startup
{
    public class UmbracoStartup : ApplicationEventHandler
    {
        private const string AppSettingKey = "WorkflowInstalled";

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext context)
        {
            //Check to see if appSetting is true or even present
            string installAppSetting = WebConfigurationManager.AppSettings[AppSettingKey];

            if (string.IsNullOrEmpty(installAppSetting))
            {
                //Check to see if section needs to be added
                Installer.AddSection(context);

                //Add Section Dashboard XML
                Installer.AddSectionDashboard();

                //Add Content dashboard XML
                Installer.AddContentSectionDashboard();

                // Grant the admin group access to the worfklow section
                //since the app is starting, we don't have a current user. Safest assumption is the installer was an admin
                IUserGroup adminGroup = context.Services.UserService.GetUserGroupByAlias("admin");
                adminGroup.AddAllowedSection("workflow");
                context.Services.UserService.Save(adminGroup, null, false);

                //All done installing our custom stuff
                //As we only want this to run once - not every startup of Umbraco
                Configuration webConfig = WebConfigurationManager.OpenWebConfiguration("/");
                webConfig.AppSettings.Settings.Add(AppSettingKey, true.ToString());
                webConfig.Save();

            }

            //Add OLD Style Package Event
            InstalledPackage.BeforeDelete += InstalledPackage_BeforeDelete;

            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;

            // add route for offline-preview
            RouteTable.Routes.MapUmbracoRoute(
                "OfflinePreviewRoute",
                "workflow-preview/{nodeId}/{userId}/{taskid}/{guid}",
                new
                {
                    controller = "OfflinePreview",
                    action = "Index",
                    nodeId = UrlParameter.Optional,
                    userId = UrlParameter.Optional,
                    taskId = UrlParameter.Optional,
                    guid = UrlParameter.Optional
                },
                new RouteHandler());

        }

        /// <summary>
        /// Add workflow-specific values to the servervariables dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> e)
        {
            e.Add("workflow", new Dictionary<string, object>
            {
                { "pluginPath", "/app_plugins/workflow/backoffice/" },
                { "apiBasePath", "/umbraco/backoffice/api/workflow/" }
            });
        }

        /// <summary>
        /// Uninstall Package - Before Delete (Old style events, no V6/V7 equivelant)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void InstalledPackage_BeforeDelete(InstalledPackage sender, System.EventArgs e)
        {
            //Check which package is being uninstalled
            if (sender.Data.Name != Constants.Name) return;

            //Start Uninstall - clean up process...
            Uninstaller.RemoveSection();
            Uninstaller.RemoveSectionDashboard();

            //Remove AppSetting key when all done
            Configuration webConfig = WebConfigurationManager.OpenWebConfiguration("/");
            webConfig.AppSettings.Settings.Remove(AppSettingKey);
            webConfig.Save();
        }
    }
}
