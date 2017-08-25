using System.Linq;
using System.Web.Hosting;
using System.Xml;
using Umbraco.Core;

namespace Workflow.Helpers
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
            var saveFile = false;
            const string dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            var dashboardXml = new XmlDocument();
            if (dashboardFilePath == null) return;

            dashboardXml.Load(dashboardFilePath);

            var firstTab = dashboardXml.SelectSingleNode("//section [@alias='StartupDashboardSection']/areas");

            if (firstTab != null)
            {
                const string xmlToAdd = "<tab caption='Workflow'>" +
                                        "<control addPanel='true' panelCaption=''>/app_plugins/workflow/backoffice/views/workflow.userdashboard.html</control>" +
                                        "</tab>";

                //Load in the XML string above
                var frag = dashboardXml.CreateDocumentFragment();
                frag.InnerXml = xmlToAdd;

                //Append the xml above to the dashboard node
                var selectSingleNode = dashboardXml.SelectSingleNode("//section [@alias='StartupDashboardSection']");
                selectSingleNode?.InsertAfter(frag, firstTab);

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
            var saveFile = false;
            const string dashboardPath = "~/config/dashboard.config";

            //Path to the file resolved
            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

            //Load settings.config XML file
            var dashboardXml = new XmlDocument();
            if (dashboardFilePath == null) return;
            dashboardXml.Load(dashboardFilePath);

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
                                        "<control>/app_plugins/workflow/backoffice/views/workflow.admindashboard.html</control>" +
                                        "</tab>" +
                                        "<tab caption=\"Documentation\">" +
                                        "<control>/app_plugins/workflow/backoffice/views/workflow.docsdashboard.html</control>" +
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
                dashboardXml.Save(dashboardFilePath);
            }
        }
    }
}
