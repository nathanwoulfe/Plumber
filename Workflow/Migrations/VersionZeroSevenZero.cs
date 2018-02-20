using System.Reflection;
using System.Web.Hosting;
using System.Xml;
using log4net;
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
            // add custom logger config

            const string configPath = "~/config/log4net.config";

            //Path to the file resolved
            string configFilePath = HostingEnvironment.MapPath(configPath);
            if (configFilePath == null) return;


            //Load log4net.config XML file
            var configXml = new XmlDocument();
            configXml.Load(configFilePath);

            XmlNode root = configXml.SelectSingleNode("./log4net");

            if (root == null) return;

            //Let's add the xml
            const string newAppender =
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

            const string newLogger = 
                "<logger name=\"Workflow\">" +
                "  <level value =\"DEBUG\" />" +
                "  <appender-ref ref=\"WorkflowLogAppender\" />" +
                "</logger>";

            //Load in the appender XML string above
            var appenderNodeToAdd = new XmlDocument();
            appenderNodeToAdd.LoadXml(newAppender);

            XmlNode appenderXmlNode = appenderNodeToAdd.SelectSingleNode("*");

            //Append the appender above to the root node
            if (appenderXmlNode != null && root.OwnerDocument != null)
            {
                root.AppendChild(root.OwnerDocument.ImportNode(appenderXmlNode, true));
            }

            // repeat for the logger
            //Load in the XML string above
            var loggerNodeToAdd = new XmlDocument();
            loggerNodeToAdd.LoadXml(newLogger);

            XmlNode loggerXmlNode = loggerNodeToAdd.SelectSingleNode("*");

            //Append the appender above to the root node
            if (loggerXmlNode != null && root.OwnerDocument != null)
            {
                root.AppendChild(root.OwnerDocument.ImportNode(loggerXmlNode, true));
            }

            //Save the XML file
            configXml.Save(configFilePath);
        }
    }
}
