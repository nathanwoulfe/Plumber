using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core;
using Umbraco.Web;
using Workflow;

namespace Workflow.Models
{
    [TableName("WorkflowUserGroupPermissions")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class UserGroupPermissionsPoco
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("GroupId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int GroupId { get; set; }

        [Column("NodeId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int NodeId { get; set; }

        [Column("Permission")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int Permission { get; set; }

        [ResultColumn]
        public string NodeName
        {
            get
            {
                return Helpers.GetNode(NodeId).Name;
            }
        }

        [ResultColumn]
        public string Url
        {
            get
            {
                return Helpers.GetNode(NodeId).Url;
            }
        }

        [ResultColumn]
        public UserGroupPoco UserGroup { 
            get 
            {
                return Helpers.GetUserGroup(GroupId);
            }
        }
    }  
}
