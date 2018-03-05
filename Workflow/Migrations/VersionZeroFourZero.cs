using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Migrations
{
	[Migration("0.4.0", 1, MagicStrings.Name)]
    public class VersionZeroFourZero : MigrationBase
    {
        private static readonly IInstancesService InstancesService = new InstancesService();

        public VersionZeroFourZero(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            ColumnInfo[] columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("WorkflowInstance") && x.ColumnName.InvariantEquals("CompletedDate")) == false)
            {
                Create.Column("CompletedDate").OnTable("WorkflowInstance").AsDateTime().Nullable();
            }

            // once the column has been added, check for any instances where status is not active, find the last task, and set complete date to match
            // this only impacts on charting, but allows more complete history as instances didn't previously store a completion date

            var instances = InstancesService.GetAll()
                .OrderByDescending(x => x.CreatedDate)
                .Where(x => x.Status != (int)WorkflowStatus.PendingApproval && x.Status != (int)WorkflowStatus.NotRequired)
                .ToList();

            foreach (var instance in instances)
            {
                var finalTask = instance.TaskInstances.LastOrDefault();
                if (null != finalTask)
                {
                    instance.CompletedDate = finalTask.CompletedDate;
                    Context.Database.Update(instance);
                }
            }
        }
    }
}
