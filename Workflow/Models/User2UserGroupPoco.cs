using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Workflow.Models
{
    [TableName("WorkflowUser2UserGroup")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class User2UserGroupPoco
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("GroupId")]
        public int GroupId { get; set; }

        [ResultColumn]
        public string Name
        {
            get
            {
                return Utility.GetUser(UserId).Name;
            }
        }

        [ResultColumn]
        public IUser User
        {
            get
            {
                return Utility.GetUser(UserId);
            }
        }
    }
}