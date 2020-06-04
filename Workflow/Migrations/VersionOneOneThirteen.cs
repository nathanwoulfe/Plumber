using System.Web.Hosting;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Constants = Workflow.Helpers.Constants;

namespace Workflow.Migrations
{
    [Migration("1.1.13", 1, Constants.Name)]
    public class VersionOneOneThirteen : MigrationBase
    {

        public VersionOneOneThirteen(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Down()
        {
        }

        /// <summary>
        /// Async log appender is obsolete, revert it to default file appender 
        /// </summary>
        public override void Up()
        {
            // Path to the file resolved
            string configFilePath = HostingEnvironment.MapPath("~/config/log4net.config");
            if (configFilePath == null) return;

            //Load config XML file
            var configXml = new XmlDocument();
            configXml.Load(configFilePath);

            string content = configXml.OuterXml;
            content = content.Replace("<appender name=\"WorkflowLogAppender\" type=\"Umbraco.Core.Logging.AsynchronousRollingFileAppender, Umbraco.Core\">",
                "<appender name=\"WorkflowLogAppender\" type=\"log4net.Appender.RollingFileAppender\">");

            configXml.LoadXml(content);
            configXml.Save(configFilePath);
        }
    }
}
