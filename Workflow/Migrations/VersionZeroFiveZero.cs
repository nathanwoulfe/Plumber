using System.Web.Hosting;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Workflow.Helpers;

namespace Workflow.Migrations
{
	[Migration("0.5.0", 1, Constants.Name)]
    public class VersionZeroFiveZero : MigrationBase
    {

        public VersionZeroFiveZero(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            // remove all workflow trees, correct version will be reinstalled later. 
            // v0.5.0 changes the way the tree is registered and rendered - the three nodes are part of one tree, not three separate trees

            const string treesPath = "~/config/trees.config";

            //Path to the file resolved
            string treesFilePath = HostingEnvironment.MapPath(treesPath);
            if (treesFilePath == null) return;


            //Load trees.config XML file
            var configXml = new XmlDocument();
            configXml.Load(treesFilePath);

            XmlNode root = configXml.SelectSingleNode("./trees");

            // get all tree nodes for workflow application - these are legacy and can be binned
            // other than the tree alias = workflow
            XmlNodeList treeNodes = root?.SelectNodes("//add[@application = 'workflow' and @alias != 'workflow']");
            if (treeNodes == null) return;

            //Let's remove the key from XML...
            foreach (XmlNode node in treeNodes)
            {
                root.RemoveChild(node);
            }

            //Save the XML file
            configXml.Save(treesFilePath);
        }
    }
}
