using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Web;

namespace Workflow.Models
{
    [TableName("WorkflowInstance")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class WorkflowInstancePoco
    {
        private IPublishedContent _node;
        private IUser _authorUser;
        private IUser _currentUser = UmbracoContext.Current.Security.CurrentUser;

        public WorkflowInstancePoco()
        {
            TaskInstances = new HashSet<WorkflowTaskInstancePoco>();
            Status = (int)WorkflowStatus.New;
            CreatedDate = DateTime.Now;
            CompletedDate = null;
        }

        public WorkflowInstancePoco(int nodeId, int authorUserId, string authorComment, WorkflowType type) : this()
        {
            NodeId = nodeId;
            AuthorUserId = authorUserId;
            AuthorComment = authorComment;
            Type = (int)type;
            SetScheduledDate();
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
            //if (Type == WorkflowType.Publish && _node.ReleaseDate.Ticks != 0)
            //{
            //    ScheduledDate = Document.ReleaseDate;
            //}
            //else if (Type == WorkflowType.Unpublish && Document.ExpireDate.Ticks != 0)
            //{
            //    ScheduledDate = Document.ExpireDate;
            //}
            //else
            //{
            //    ScheduledDate = null;
            //}
            ScheduledDate = null;
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
                    _node = Helpers.GetNode(NodeId);
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
                    _authorUser = Helpers.GetUser(AuthorUserId);
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
                return WorkflowStatusName(_Status);
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
                return _Status != WorkflowStatus.Cancelled
                    && _Status != WorkflowStatus.Completed
                    && _Status != WorkflowStatus.Errored
                    && _Status != WorkflowStatus.Rejected;
            }
        }



         //<summary>
         
         //</summary>
         //<param name="userId">The id of the user to check</param>
         //<returns></returns>
        public bool IsUserInWorkflow(int userId)
        {
            if (AuthorUserId == userId) 
                return true;

            foreach (WorkflowTaskInstancePoco taskInstance in TaskInstances)
            {
                if (taskInstance.UserGroup != null && taskInstance.UserGroup.IsMember(userId)) 
                    return true;
            }
            // If we get here they mustnt be involved in the workflow.
            return false;
        }

         //<summary>
         //Determine if the specified user can approve or reject the current workflow task instance.
         //</summary>
         //<param name="userId"></param>
         //<returns>true if there is an active task and the user is in the group responsible for approving it - otherwise false</returns>
        public bool CanUserActionWorkflow(int userId)
        {
            WorkflowTaskInstancePoco ti = ActiveTask;
            return ti != null && ti.UserGroup.IsMember(userId);
        }

         //<summary>
         //Get the active task instance pending approval - or null if none.
         //</summary>
        [ResultColumn]        
        public WorkflowTaskInstancePoco ActiveTask
        {
            get
            {
                return TaskInstances != null ? TaskInstances.FirstOrDefault(ti => ti._Status == Workflow.Models.TaskStatus.PendingApproval) : null;
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
            return Helpers.PascalCaseToTitleCase(type.ToString());
        }

        public static string EmailTypeName(EmailType type)
        {
            return Helpers.PascalCaseToTitleCase(type.ToString());
        }

        public static string WorkflowStatusName(WorkflowStatus status)
        {
            return Helpers.PascalCaseToTitleCase(status.ToString());
        }

        public bool CanCurrentUserActionWorkflow()
        {
            return CanUserActionWorkflow(_currentUser.Id);
        }

        public bool CanCurrentUserCancelWorkflow()
        {
            return Active && (IsUserInWorkflow(_currentUser.Id) || Helpers.IUserCanAdminWorkflow(_currentUser));
        }
    }
}
