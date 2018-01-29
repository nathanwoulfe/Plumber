using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Workflow.Models
{
    [TableName("WorkflowSettings")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class WorkflowSettingsPoco
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("DefaultApprover")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string DefaultApprover { get; set; }

        [Column("Email")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Email { get; set; }

        [Column("EditUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string EditUrl { get; set; }

        [Column("SiteUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SiteUrl { get; set; }

        [Column("FlowType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int FlowType { get; set; }

        [Column("SendNotifications")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public bool SendNotifications { get; set; }

        [Column("ExcludeNodes")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ExcludeNodes { get; set; }
    }
}
