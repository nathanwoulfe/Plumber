using System.Linq;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace Workflow.Helpers
{
    public static class Installer
    {
        /// <summary>W
        /// Adds the application/custom section to Umbraco
        /// </summary>
        /// <param name="applicationContext"></param>
        public static bool AddSection(ApplicationContext applicationContext)
        {
            //Get SectionService
            var sectionService = applicationContext.Services.SectionService;

            //Try & find a section with the alias of "workflow"
            var workflowSection = sectionService.GetSections().SingleOrDefault(x => x.Alias == "workflow");

            //If we can't find the section - doesn't exist
            if (workflowSection == null)
            {
                //So let's create it the section
                sectionService.MakeNew("Workflow", "workflow", "icon-path");
                return true;
            }

            return false;
        }

        public static bool AddContentSectionDashboard()
        {
            var saveFile = false;

            //Path to the file resolved
            var dashboardXml = XmlHelper.OpenAsXmlDocument(SystemFiles.DashboardConfig);

            if (dashboardXml == null) return false;

            // Section Node
            var findSection = dashboardXml.SelectSingleNode("//section [@alias='WorkflowContentDashboardSection']");

            //Couldn't find it
            if (findSection == null)
            {
                //Let's add the xml
                const string xmlToAdd = "<section alias='WorkflowContentDashboardSection'>" +
                                        "<areas>" +
                                        "<area>content</area>" +
                                        "</areas>" +
                                        "<tab caption=\"Workflow\">" +
                                        "<control>../app_plugins/workflow/backoffice/views/workflow.userdashboard.html</control>" +
                                        "</tab>" +
                                        "</section>";

                //Get the main root <dashboard> node
                var dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");

                if (dashboardNode != null)
                {
                    //Load in the XML string above
                    var xmlNodeToAdd = new XmlDocument();
                    xmlNodeToAdd.LoadXml(xmlToAdd);

                    var toAdd = xmlNodeToAdd.SelectSingleNode("*");

                    //Append the xml above to the dashboard node
                    if (toAdd != null && dashboardNode.OwnerDocument != null)
                    {
                        dashboardNode.AppendChild(dashboardNode.OwnerDocument.ImportNode(toAdd, true));
                    }

                    //Save the file flag to true
                    saveFile = true;
                }
            }

            //If saveFile flag is true then save the file
            if (saveFile)
            {
                //Save the XML file
                dashboardXml.Save(IOHelper.MapPath(SystemFiles.DashboardConfig));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the required XML to the dashboard.config file
        /// </summary>
        public static bool AddSectionDashboard()
        {
            var saveFile = false;

            var dashboardXml = XmlHelper.OpenAsXmlDocument(SystemFiles.DashboardConfig);

            //Load settings.config XML file
            if (dashboardXml == null) return false;

            // Section Node
            var findSection = dashboardXml.SelectSingleNode("//section [@alias='WorkflowDashboardSection']");

            //Couldn't find it
            if (findSection == null)
            {
                //Let's add the xml
                const string xmlToAdd = "<section alias='WorkflowDashboardSection'>" +
                                        "<areas>" +
                                        "<area>workflow</area>" +
                                        "</areas>" +
                                        "<tab caption=\"Overview\">" +
                                        "<control>../app_plugins/workflow/backoffice/views/workflow.admindashboard.html</control>" +
                                        "</tab>" +
                                        "<tab caption=\"Documentation\">" +
                                        "<control>../app_plugins/workflow/backoffice/views/workflow.docsdashboard.html</control>" +
                                        "</tab>" +
                                        "<tab caption=\"Log viewer\">" +
                                        "<control>../app_plugins/workflow/backoffice/views/workflow.logsdashboard.html</control>" +
                                        "</tab>" +
                                        "<tab caption=\"Deployment\">" +
                                        "<control>../app_plugins/workflow/backoffice/views/workflow.importexportdashboard.html</control>" +
                                        "</tab>" +
                                        "</section>";

                //Get the main root <dashboard> node
                var dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");

                if (dashboardNode != null)
                {
                    //Load in the XML string above
                    var xmlNodeToAdd = new XmlDocument();
                    xmlNodeToAdd.LoadXml(xmlToAdd);

                    var toAdd = xmlNodeToAdd.SelectSingleNode("*");

                    //Append the xml above to the dashboard node
                    if (toAdd != null && dashboardNode.OwnerDocument != null)
                    {
                        dashboardNode.AppendChild(dashboardNode.OwnerDocument.ImportNode(toAdd, true));
                    }

                    //Save the file flag to true
                    saveFile = true;
                }
            }

            //If saveFile flag is true then save the file
            if (saveFile)
            {
                //Save the XML file
                dashboardXml.Save(IOHelper.MapPath(SystemFiles.DashboardConfig));
                return true;
            }

            return false;
        }
    }
}
