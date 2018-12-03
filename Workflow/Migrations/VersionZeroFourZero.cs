using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Constants = Workflow.Helpers.Constants;

namespace Workflow.Migrations
{
    [Migration("0.4.0", 1, Constants.Name)]
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

            if (columns.Any(x => x.TableName.InvariantEquals("WorkflowInstance") && x.ColumnName.InvariantEquals("CompletedDate"))) return;

            // column doesn't exist, add it and populate the completed date for any existing instances

            Create.Column("CompletedDate").OnTable("WorkflowInstance").AsDateTime().Nullable();

            // once the column has been added, check for any instances where status is not active, find the last task, and set complete date to match
            // this only impacts on charting, but allows more complete history as instances didn't previously store a completion date

            List<WorkflowInstancePoco> instances = InstancesService.GetAll()
                .Where(x => x.Status == (int)WorkflowStatus.Approved || x.Status == (int)WorkflowStatus.Cancelled)
                .ToList();

            if (!instances.Any()) return;

            foreach (WorkflowInstancePoco instance in instances)
            {
                if (!instance.TaskInstances.Any()) continue;

                WorkflowTaskInstancePoco finalTask = instance.TaskInstances.OrderBy(x => x.Id).Last();

                instance.CompletedDate = finalTask.CompletedDate;
                Context.Database.Update(instance);
            }

        }
    }
}
