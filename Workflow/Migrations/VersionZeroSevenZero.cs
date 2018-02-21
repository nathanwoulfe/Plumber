using System.Web.Hosting;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Workflow.Helpers;

namespace Workflow.Migrations
{
    [Migration("0.7.0", 1, MagicStrings.Name)]
    public class VersionZeroSevenZero : MigrationBase
    {

        public VersionZeroSevenZero(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            // define the new elements to add
            const string appender =
                "<appender name=\"WorkflowLogAppender\" type=\"Umbraco.Core.Logging.AsynchronousRollingFileAppender, Umbraco.Core\">" +
                "  <file value=\"App_Data\\Logs\\WorkflowLog.txt\" />" +
                "  <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />" +
                "  <appendToFile value=\"true\" />" +
                "  <rollingStyle value=\"Date\" />" +
                "  <maximumFileSize value=\"5MB\" />" +
                "  <layout type=\"log4net.Layout.PatternLayout\">" +
                "    <conversionPattern value=\"%date [%thread] %-5level %logger - %message %newline\" />" +
                "  </layout>" +
                "</appender>";

            const string logger =
                "<logger name=\"Workflow\">" +
                "  <level value=\"DEBUG\" />" +
                "  <appender-ref ref=\"WorkflowLogAppender\" />" +
                "</logger>";

            const string dashboard =
                "<tab caption=\"Log viewer\">" +
                "  <control>/app_plugins/workflow/backoffice/views/workflow.logsdashboard.html</control>" +
                "</tab>";

            // update the respective config files
            UpdateConfigFile("~/config/log4net.config", "./log4net", logger);
            UpdateConfigFile("~/config/log4net.config", "./log4net", appender);
            UpdateConfigFile("~/config/Dashboard.config", "//section [@alias='WorkflowDashboardSection']", dashboard);
        }

        /// <summary>
        /// Helper for updating xml config files
        /// </summary>
        /// <param name="configPath">The path to the config file</param>
        /// <param name="rootNode">Parent node for the new content</param>
        /// <param name="xmlToAdd">XML-like string to add</param>
        private static void UpdateConfigFile(string configPath, string rootNode, string xmlToAdd)
        {
            //Path to the file resolved
            string configFilePath = HostingEnvironment.MapPath(configPath);
            if (configFilePath == null) return;

            //Load config XML file
            var configXml = new XmlDocument();
            configXml.Load(configFilePath);

            // parent Node
            XmlNode parent = configXml.SelectSingleNode(rootNode);

            if (parent == null) return;
 
            //Load in the XML string
            var xmlNodeToAdd = new XmlDocument();
            xmlNodeToAdd.LoadXml(xmlToAdd);

            XmlNode toAdd = xmlNodeToAdd.SelectSingleNode("*");

            //Append the xml to the root node
            if (toAdd != null && parent.OwnerDocument != null)
            {
                parent.AppendChild(parent.OwnerDocument.ImportNode(toAdd, true));
            }

            //Save the XML file
            configXml.Save(configFilePath);
            
        }
    }
}
