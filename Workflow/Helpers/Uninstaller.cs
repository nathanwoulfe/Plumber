using System.Web.Hosting;
using System.Xml;
using Umbraco.Core;

namespace Workflow.Helpers
{
    public class Uninstaller
    {
        /// <summary>
        /// Removes the custom app/section from Umbraco
        /// </summary>
        public void RemoveSection()
        {
            //Get the Umbraco Service's Apis
            var services = ApplicationContext.Current.Services;

            //Check to see if the section is still here (should be)
            var workflowSection = services.SectionService.GetByAlias("workflow");

            if (workflowSection != null)
            {
                //Delete the section from the application
                services.SectionService.DeleteSection(workflowSection);
            }
        }

        /// <summary>
        /// Removes the XML for the Section Dashboard from the XML file
        /// </summary>
        public void RemoveSectionDashboard()
        {
            var saveFile = false;

            //Open up language file
            //umbraco/config/lang/en.xml
            const string dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            var dashboardXml = new XmlDocument();
            if (dashboardFilePath == null) return;

            dashboardXml.Load(dashboardFilePath);

            // Dashboard Root Node
            // <dashboard>
            var dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");

            var findSectionKey = dashboardNode?.SelectSingleNode("./section [@alias='WorkflowDashboardSection']");

            if (findSectionKey != null)
            {
                //Let's remove the key from XML...
                dashboardNode.RemoveChild(findSectionKey);

                //Save the file flag to true
                saveFile = true;
            }

            var contentTab = dashboardNode?.SelectSingleNode("//tab[@caption='Workflow']");

            if (contentTab != null)
            {
                contentTab.ParentNode?.RemoveChild(contentTab);
                saveFile = true;
            }

            //If saveFile flag is true then save the file
            if (!saveFile) return;

            //Save the XML file
            dashboardXml.Save(dashboardFilePath);
        }
    }
}