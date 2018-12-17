using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Workflow.Helpers;

namespace Workflow.Models
{
    [TableName("WorkflowUser2UserGroup")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class User2UserGroupPoco
    {
        private readonly Utility _utility = new Utility();

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("GroupId")]
        public int GroupId { get; set; }

        //[ResultColumn]
        //public string Name => _utility.GetUser(UserId)?.Name ?? string.Empty;

        [ResultColumn]
        public IUser User => _utility.GetUser(UserId);
    }
}