using System;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Workflow.Extensions;
using Workflow.Helpers;

namespace Workflow.Models
{
    [TableName("WorkflowTaskInstance")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class WorkflowTaskPoco
    {
        private IUser _actionedByUser;
        private readonly Utility _utility;

        public WorkflowTaskPoco()
        {
            CreatedDate = DateTime.Now;
            CompletedDate = null;
            Status = (int)Models.TaskStatus.PendingApproval;
            ApprovalStep = 0;
            ActionedByAdmin = false;

            _utility = new Utility();
        }

        public WorkflowTaskPoco(TaskType type) : this()
        {
            Type = (int)type;
        }

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("Type")]
        public int Type { get; set; }

        [Column("ApprovalStep")]
        public int ApprovalStep { get; set; }

        [Column("WorkflowInstanceGuid")]
        public Guid WorkflowInstanceGuid { get; set; }

        [Column("GroupId")]
        public int GroupId { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("Status")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int Status { get; set; }

        [Column("Comment")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Comment { get; set; }

        [Column("CompletedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? CompletedDate { get; set; }

        [Column("ActionedByUserId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? ActionedByUserId { get; set; }

        [Column("ActionedByAdmin")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public bool ActionedByAdmin { get; set; }

        [ResultColumn]
        public TaskStatus? TaskStatus => (TaskStatus?)Status;

        [ResultColumn]
        public IUser ActionedByUser
        {
            get
            {
                if (_actionedByUser == null && ActionedByUserId.HasValue)
                {
                    _actionedByUser = _utility.GetUser(ActionedByUserId.Value);
                }
                return _actionedByUser;
            }
        }

        [ResultColumn]
        public string StatusName => TaskStatus.ToString().ToTitleCase();

        [ResultColumn]
        public virtual UserGroupPoco UserGroup { get; set; }

        [ResultColumn]
        public virtual WorkflowInstancePoco WorkflowInstance { get; set; }
    }
}
