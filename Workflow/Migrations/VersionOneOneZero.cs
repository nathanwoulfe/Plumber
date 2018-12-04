using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Constants = Workflow.Helpers.Constants;

namespace Workflow.Migrations
{
    [Migration("1.1.0", 1, Constants.Name)]
    public class VersionOneOneZero : MigrationBase
    {

        public VersionOneOneZero(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            ColumnInfo[] columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals(Constants.PermissionsTable) && x.ColumnName.InvariantEquals("Condition")) == false)
            {
                Create.Column("Condition").OnTable(Constants.PermissionsTable).AsString().Nullable();
            }
        }
    }
}
