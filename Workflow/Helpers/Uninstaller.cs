using System.Web.Hosting;
using System.Xml;
using Umbraco.Web;

namespace Workflow
{
    public class Uninstaller
    {
        /// <summary>
        /// Removes the custom app/section from Umbraco
        /// </summary>
        public void RemoveSection()
        {
            //Get the Umbraco Service's Apis
            var services = UmbracoContext.Current.Application.Services;

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
            bool saveFile = false;

            //Open up language file
            //umbraco/config/lang/en.xml
            var dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            XmlDocument dashboardXml = new XmlDocument();
            dashboardXml.Load(dashboardFilePath);

            // Dashboard Root Node
            // <dashboard>
            XmlNode dashboardNode = dashboardXml.SelectSingleNode("//dashboard");

            if (dashboardNode != null)
            {
                XmlNode findSectionKey = dashboardNode.SelectSingleNode("./section [@alias='WorkflowDashboardSection']");

                if (findSectionKey != null)
                {
                    //Let's remove the key from XML...
                    dashboardNode.RemoveChild(findSectionKey);

                    //Save the file flag to true
                    saveFile = true;
                }
            }

            XmlNode contentTab = dashboardXml.SelectSingleNode("//section [@alias='StartupDashboardSection']/areas/tab [@caption='Workflow']");

            if (contentTab != null)
            {
                dashboardNode.RemoveChild(contentTab);
                saveFile = true;
            }

            //If saveFile flag is true then save the file
            if (saveFile)
            {
                //Save the XML file
                dashboardXml.Save(dashboardFilePath);
                saveFile = false;
            }
        }
    }
}