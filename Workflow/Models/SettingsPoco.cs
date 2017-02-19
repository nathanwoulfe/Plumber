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

        [Column("FinalApprover")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string FinalApprover { get; set; }

        [Column("FastTrack")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string FastTrack { get; set; }

        [Column("Email")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Email { get; set; }

        [Column("EditUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string EditUrl { get; set; }

        [Column("SiteUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SiteUrl { get; set; }
    }
}
