using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;
using Umbraco.Core;

namespace Workflow
{
    public class Installer
    {
        /// <summary>
        /// Adds the application/custom section to Umbraco
        /// </summary>
        /// <param name="applicationContext"></param>
        public void AddSection(ApplicationContext applicationContext)
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
            }
        }

        public void AddContentSectionDashboard()
        {
            bool saveFile = false;
            var dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            XmlDocument dashboardXml = new XmlDocument();
            dashboardXml.Load(dashboardFilePath);

            XmlNode firstTab = dashboardXml.SelectSingleNode("//section [@alias='StartupDashboardSection']/areas");

            if (firstTab != null)
            {
                var xmlToAdd = "<tab caption='Workflow'>" +
                                    "<control addPanel='true' panelCaption=''>/app_plugins/plumber/backoffice/views/workflow.userdashboard.html</control>" +
                                "</tab>";

                //Load in the XML string above
                XmlDocumentFragment frag = dashboardXml.CreateDocumentFragment();
                frag.InnerXml = xmlToAdd;

                //Append the xml above to the dashboard node
                dashboardXml.SelectSingleNode("//section [@alias='StartupDashboardSection']").InsertAfter(frag, firstTab);

                //Save the file flag to true
                saveFile = true;
            }

            //If saveFile flag is true then save the file
            if (saveFile)
            {
                //Save the XML file
                dashboardXml.Save(dashboardFilePath);
            }
        }

        /// <summary>
        /// Adds the required XML to the dashboard.config file
        /// </summary>
        public void AddSectionDashboard()
        {
            bool saveFile = false;
            var dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            XmlDocument dashboardXml = new XmlDocument();
            dashboardXml.Load(dashboardFilePath);

            // Section Node
            XmlNode findSection = dashboardXml.SelectSingleNode("//section [@alias='WorkflowDashboardSection']");

            //Couldn't find it
            if (findSection == null)
            {
                //Let's add the xml
                var xmlToAdd = "<section alias='WorkflowDashboardSection'>" +
                                    "<areas>" +
                                      "<area>workflow</area>" +
                                    "</areas>" +
                                    "<tab caption=\"Overview\">" +
                                      "<control>/app_plugins/plumber/backoffice/views/workflow.admindashboard.html</control>" +
                                    "</tab>" +
                                    "<tab caption=\"Documentation\">" +
                                      "<control>/app_plugins/plumber/backoffice/views/workflow.docsdashboard.html</control>" +
                                    "</tab>" +
                                  "</section>";

                //Get the main root <dashboard> node
                XmlNode dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");

                if (dashboardNode != null)
                {
                    //Load in the XML string above
                    XmlDocument xmlNodeToAdd = new XmlDocument();
                    xmlNodeToAdd.LoadXml(xmlToAdd);

                    var toAdd = xmlNodeToAdd.SelectSingleNode("*");

                    //Append the xml above to the dashboard node
                    dashboardNode.AppendChild(dashboardNode.OwnerDocument.ImportNode(toAdd, true));

                    //Save the file flag to true
                    saveFile = true;
                }
            }

            //If saveFile flag is true then save the file
            if (saveFile)
            {
                //Save the XML file
                dashboardXml.Save(dashboardFilePath);
            }
        }
    }
}
