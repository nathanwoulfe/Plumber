using Workflow.Models;

namespace Workflow.Events.Args
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupEventArgs : EventArgsBase
    {
        public GroupEventArgs(UserGroupPoco group)
        {
            Group = group;
        }

        private UserGroupPoco Group { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupDeletedEventArgs : EventArgsBase
    {
        public GroupDeletedEventArgs(int groupId)
        {
            GroupId = groupId;
        }

        private int GroupId { get; set; }
    }
}
