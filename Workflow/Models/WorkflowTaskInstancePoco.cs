using System;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Workflow.Models
{
    [TableName("WorkflowTaskInstance")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class WorkflowTaskInstancePoco
    {
        private IUser _actionedByUser;

        public WorkflowTaskInstancePoco()
        {
            CreatedDate = DateTime.Now;
            CompletedDate = null;
            Status = (int)TaskStatus.New;
            ApprovalStep = 0;
        }

        public WorkflowTaskInstancePoco(TaskType type)
            : this()
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

        [ResultColumn]
        public TaskStatus? _Status
        {
            get
            {
                return (TaskStatus?)Status;
            }
        }

        [ResultColumn]
        public TaskType _Type
        {
            get
            {
                return (TaskType)Type;
            }
        }

        [ResultColumn]
        public IUser ActionedByUser
        {
            get
            {
                if (_actionedByUser == null && ActionedByUserId.HasValue)
                {
                    _actionedByUser = Helpers.GetUser(ActionedByUserId.Value);
                }
                return _actionedByUser;
            }
        }

        [ResultColumn]
        public string TypeName
        {
            get
            {
                return Helpers.PascalCaseToTitleCase(Type.ToString());
            }
        }

        [ResultColumn]
        public string StatusName
        {
            get
            {
                return Helpers.PascalCaseToTitleCase(_Status.ToString()); ;
            }
        }

        [ResultColumn]
        public virtual UserGroupPoco UserGroup { get; set; }

        [ResultColumn]
        public virtual WorkflowInstancePoco WorkflowInstance { get; set; }
        

        /// <summary>
        /// Indicates whether the task instance is currently active.
        /// </summary>        
        [ResultColumn]
        public bool Active
        {
            get { return _Status == TaskStatus.New || _Status == TaskStatus.PendingApproval; }
        }
    }
}
