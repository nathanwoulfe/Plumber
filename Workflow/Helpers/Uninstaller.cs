using Umbraco.Core;
using Umbraco.Core.IO;

namespace Workflow.Helpers
{
    public static class Uninstaller
    {
        /// <summary>
        /// Removes the custom app/section from Umbraco
        /// </summary>
        public static void RemoveSection()
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
        public static void RemoveSectionDashboard()
        {
            var saveFile = false;

            //Path to the file resolved
            var dashboardXml = XmlHelper.OpenAsXmlDocument(SystemFiles.DashboardConfig);

            if (dashboardXml == null) return;


            // Dashboard Root Node
            // <dashboard>
            var dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");

            var findSectionKey = dashboardNode?.SelectSingleNode("//section [@alias='WorkflowContentDashboardSection']");

            if (findSectionKey != null)
            {
                //Let's remove the key from XML...
                dashboardNode.RemoveChild(findSectionKey);

                //Save the file flag to true
                saveFile = true;
            }

            findSectionKey = dashboardNode?.SelectSingleNode("//section [@alias='WorkflowDashboardSection']");

            if (findSectionKey != null)
            {
                //Let's remove the key from XML...
                dashboardNode.RemoveChild(findSectionKey);

                //Save the file flag to true
                saveFile = true;
            }

            //If saveFile flag is true then save the file
            if (!saveFile) return;

            //Save the XML file
            dashboardXml.Save(IOHelper.MapPath(SystemFiles.DashboardConfig));
        }
    }
}