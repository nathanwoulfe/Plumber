using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Services;
using Umbraco.Web;
using Workflow.Extensions;

namespace Workflow.Models
{
    [TableName("WorkflowInstance")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class WorkflowInstancePoco
    {
        private IPublishedContent _node;
        private IUser _authorUser;
        private IContentService _cs = ApplicationContext.Current.Services.ContentService;

        public WorkflowInstancePoco()
        {
            TaskInstances = new HashSet<WorkflowTaskInstancePoco>();
            Status = (int)WorkflowStatus.PendingApproval;
            CreatedDate = DateTime.Now;
            CompletedDate = null;
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

        [Column("AuthorComment")]
        public string AuthorComment { get; set; }

        [ResultColumn]
        public WorkflowStatus _Status
        {
            get
            {
                return (WorkflowStatus)Status;
            }
        }

        [ResultColumn]
        public WorkflowType _Type
        {
            get
            {
                return (WorkflowType)Type;
            }
        }

        public void SetScheduledDate()
        {
            var content = _cs.GetById(NodeId);
            if (Type ==  (int)WorkflowType.Publish && content.ReleaseDate.HasValue)
            {
                ScheduledDate = content.ReleaseDate;
            }
            else if (Type == (int)WorkflowType.Unpublish && content.ExpireDate.HasValue)
            {
                ScheduledDate = content.ExpireDate;
            }
            else
            {
                ScheduledDate = null;
            }
        }

        /// <summary>
        /// Title case text name for the workflow type.
        /// </summary>
        [ResultColumn]
        public string TypeName
        {
            get
            {
                return WorkflowTypeName(_Type);
            }
        }

        [ResultColumn]
        public string TypeDescriptionPastTense
        {
            get
            {
                return TypeDescription.Replace("ish", "ished").Replace("dule", "duled").Replace("for", "to be");
            }
        }

        /// <summary>
        /// Describe the workflow type by including details for release at / expire at scheduling.
        /// </summary>
        [ResultColumn]
        public string TypeDescription
        {
            get
            {
                return WorkflowTypeDescription(_Type, ScheduledDate);
            }
        }

        /// <summary>
        /// The document object associated with this workflow.
        /// </summary>
        [ResultColumn]
        public IPublishedContent Node
        {
            get
            {
                if (_node == null)
                {
                    _node = Utility.GetNode(NodeId);
                }
                return _node;
            } 
        }

        /// <summary>
        /// The author user who initiated this workflow instance.
        /// </summary>
        [ResultColumn]
        public IUser AuthorUser
        {
            get
            {
                if (_authorUser == null)
                {
                    _authorUser = Utility.GetUser(AuthorUserId);
                }
                return _authorUser;
            }
        }

        /// <summary>
        /// Title case text name for the workflow status.
        /// </summary>
        [ResultColumn]       
        public string StatusName
        {
            get
            {
                return Utility.PascalCaseToTitleCase(_Status.ToString()); ;
            }
        }

        /// <summary>
        /// Indicates whether the workflow instance is currently active.
        /// </summary>
        [ResultColumn]        
        public bool Active
        {
            get
            {
                return _Status != WorkflowStatus.Cancelled && _Status != WorkflowStatus.Rejected;
            }
        }

        [ResultColumn]
        public Nullable<DateTime> CompletedDate { get; set; }

        [ResultColumn]
        public Nullable<DateTime> ScheduledDate { get; set; }

        [ResultColumn]
        public virtual ICollection<WorkflowTaskInstancePoco> TaskInstances { get; set; }

        public static string WorkflowTypeDescription(WorkflowType type, Nullable<DateTime> scheduledDate)
        {
            if (scheduledDate.HasValue)
            {
                return "Schedule for " + WorkflowTypeName(type) + " at " + scheduledDate.Value.ToString("dd/MM/yy HH:mm");
            }
            else
            {
                return WorkflowTypeName(type);
            }
        }

        public static string WorkflowTypeName(WorkflowType type)
        {
            return Utility.PascalCaseToTitleCase(type.ToString());
        }

        public static string EmailTypeName(EmailType type)
        {
            return Utility.PascalCaseToTitleCase(type.ToString());
        }
    }
}
