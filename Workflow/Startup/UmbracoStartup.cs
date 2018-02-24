using System.Configuration;
using System.Web.Configuration;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Workflow.Helpers;

namespace Workflow.Startup
{
    public class UmbracoStartup : ApplicationEventHandler
    {
        private const string AppSettingKey = "WorkflowInstalled";

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext context)
        {
            //Check to see if appSetting AnalyticsStartupInstalled is true or even present
            string installAppSetting = WebConfigurationManager.AppSettings[AppSettingKey];

            if (string.IsNullOrEmpty(installAppSetting) || installAppSetting != true.ToString())
            {
                var install = new Helpers.Installer();

                //Check to see if section needs to be added
                install.AddSection(context);

                //Add Section Dashboard XML
                install.AddSectionDashboard();

                //Add Content dashboard XML
                install.AddContentSectionDashboard();

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
        }

        /// <summary>
        /// Uninstall Package - Before Delete (Old style events, no V6/V7 equivelant)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void InstalledPackage_BeforeDelete(InstalledPackage sender, System.EventArgs e)
        {
            //Check which package is being uninstalled
            if (sender.Data.Name != MagicStrings.Name) return;

            var uninstall = new Uninstaller();

            //Start Uninstall - clean up process...
            uninstall.RemoveSection();
            uninstall.RemoveSectionDashboard();

            //Remove AppSetting key when all done
            Configuration webConfig = WebConfigurationManager.OpenWebConfiguration("/");
            webConfig.AppSettings.Settings.Remove(AppSettingKey);
            webConfig.Save();
        }
    }
}
