using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Constants = Workflow.Helpers.Constants;

namespace Workflow.Migrations
{
    [Migration("0.9.0", 1, Constants.Name)]
    public class VersionZeroNineZero : MigrationBase
    {

        public VersionZeroNineZero(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            ColumnInfo[] columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("WorkflowUserGroups") && x.ColumnName.InvariantEquals("OfflineApproval")) == false)
            {
                Create.Column("OfflineApproval").OnTable("WorkflowUserGroups").AsBoolean().Nullable();
            }
        }
    }
}
