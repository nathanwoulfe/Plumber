using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Workflow.Helpers;

namespace Workflow.Models
{
    [TableName("WorkflowInstance")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class WorkflowInstancePoco
    {
        private IPublishedContent _node;
        private IUser _authorUser;
        private readonly Utility _utility;

        public WorkflowInstancePoco()
        {
            TaskInstances = new HashSet<WorkflowTaskInstancePoco>();
            Status = (int)WorkflowStatus.PendingApproval;
            CreatedDate = DateTime.Now;
            CompletedDate = null;

            _utility = new Utility();
        }

        public WorkflowInstancePoco(int nodeId, int authorUserId, string authorComment, WorkflowType type) : this()
        {
            NodeId = nodeId;
            AuthorUserId = authorUserId;
            AuthorComment = authorComment;
            Type = (int)type;
        }

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("Guid")]
        public Guid Guid { get; set; }

        [Column("NodeId")]
        public int NodeId { get; set; }

        [Column("Type")]
        public int Type { get; set; }

        [Column("TotalSteps")]
        public int TotalSteps { get; set; }

        [Column("AuthorUserId")]
        public int AuthorUserId { get; set; }

        [Column("Status")]
        public int Status { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("CompletedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? CompletedDate { get; set; }

        [Column("AuthorComment")]
        public string AuthorComment { get; set; }

        [ResultColumn]
        public WorkflowStatus WorkflowStatus => (WorkflowStatus)Status;

        [ResultColumn]
        public WorkflowType WorkflowType => (WorkflowType)Type;

        public void SetScheduledDate()
        {
            IContent content = _utility.GetContent(NodeId);
            switch (Type)
            {
                case (int)WorkflowType.Publish when content.ReleaseDate.HasValue:
                    ScheduledDate = content.ReleaseDate;
                    break;
                case (int)WorkflowType.Unpublish when content.ExpireDate.HasValue:
                    ScheduledDate = content.ExpireDate;
                    break;
                default:
                    ScheduledDate = null;
                    break;
            }
        }

        /// <summary>
        /// Title case text name for the workflow type.
        /// </summary>
        [ResultColumn]
        public string TypeName => WorkflowTypeName(WorkflowType);

        [ResultColumn]
        public string TypeDescriptionPastTense => TypeDescription.Replace("ish", "ished").Replace("dule", "duled").Replace("for", "to be");

        /// <summary>
        /// Describe the workflow type by including details for release at / expire at scheduling.
        /// </summary>
        [ResultColumn]
        public string TypeDescription => WorkflowTypeDescription(WorkflowType, ScheduledDate);

        /// <summary>
        /// The document object associated with this workflow.
        /// </summary>
        [ResultColumn]
        public IPublishedContent Node => _node ?? (_node = _utility.GetPublishedContent(NodeId));

        /// <summary>
        /// The author user who initiated this workflow instance.
        /// </summary>
        [ResultColumn]
        public IUser AuthorUser => _authorUser ?? (_authorUser = _utility.GetUser(AuthorUserId));

        /// <summary>
        /// Title case text name for the workflow status.
        /// </summary>
        [ResultColumn]
        public string StatusName => _utility.PascalCaseToTitleCase(WorkflowStatus.ToString());

        /// <summary>
        /// Indicates whether the workflow instance is currently active.
        /// </summary>
        [ResultColumn]
        public bool Active => WorkflowStatus != WorkflowStatus.Cancelled 
                              && WorkflowStatus != WorkflowStatus.Errored 
                              && WorkflowStatus != WorkflowStatus.Approved;

        [ResultColumn]
        public DateTime? ScheduledDate { get; set; }

        [ResultColumn]
        public ICollection<WorkflowTaskInstancePoco> TaskInstances { get; set; }

        private string WorkflowTypeDescription(WorkflowType type, DateTime? scheduledDate)
        {
            if (scheduledDate.HasValue)
            {
                return "Schedule for " + WorkflowTypeName(type) + " at " + scheduledDate.Value.ToString("dd/MM/yy HH:mm");
            }

            return WorkflowTypeName(type);
        }

        private string WorkflowTypeName(WorkflowType type)
        {
            return _utility.PascalCaseToTitleCase(type.ToString());
        }

        public string EmailTypeName(EmailType type)
        {
            return _utility.PascalCaseToTitleCase(type.ToString());
        }
    }
}
