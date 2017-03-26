using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;

namespace Workflow
{
    public class UmbracoStartup : ApplicationEventHandler
    {
        private const string AppSettingKey = "WorkflowInstalled";

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext context)
        {
            //Check to see if appSetting AnalyticsStartupInstalled is true or even present
            var installAppSetting = WebConfigurationManager.AppSettings[AppSettingKey];

            if (string.IsNullOrEmpty(installAppSetting) || installAppSetting != true.ToString())
            {
                var install = new Installer();

                //Check to see if section needs to be added
                install.AddSection(context);

                //Add Section Dashboard XML
                install.AddSectionDashboard();

                //Add Content dashboard XML
                install.AddContentSectionDashboard();

                //All done installing our custom stuff
                //As we only want this to run once - not every startup of Umbraco
                var webConfig = WebConfigurationManager.OpenWebConfiguration("/");
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
        void InstalledPackage_BeforeDelete(InstalledPackage sender, System.EventArgs e)
        {
            //Check which package is being uninstalled
            if (sender.Data.Name == "Workflow")
            {
                var uninstall = new Uninstaller();

                //Start Uninstall - clean up process...
                uninstall.RemoveSection();
                uninstall.RemoveSectionDashboard();

                //Remove AppSetting key when all done
                var webConfig = WebConfigurationManager.OpenWebConfiguration("/");
                webConfig.AppSettings.Settings.Remove(AppSettingKey);
                webConfig.Save();
            }
        }
    }
}
