using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Workflow.Helpers;

namespace Workflow.Models
{
    [TableName("WorkflowUserGroups")]
    [ExplicitColumns]
    [PrimaryKey("GroupId", autoIncrement = true)]
    public class UserGroupPoco
    {
        [Column("GroupId")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int GroupId { get; set; }

        [Column("Description")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Description { get; set; }

        [Column("Name")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Name { get; set; }

        [Column("Alias")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Alias { get; set; }

        [Column("GroupEmail")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string GroupEmail { get; set; }

        [Column("Deleted")]
        [NullSetting(NullSetting = NullSettings.NotNull)]        
        public bool Deleted { get; set; }

        [ResultColumn]
        public List<UserGroupPermissionsPoco> Permissions { get; set; }

        [ResultColumn]
        public List<User2UserGroupPoco> Users { get; set; }

        public UserGroupPoco() {
            Users = new List<User2UserGroupPoco>();
            Permissions = new List<UserGroupPermissionsPoco>();
        }

        [ResultColumn]
        public string UsersSummary {
            get
            {
                return string.Concat("|", string.Join("|", Users.Select(u => u.UserId)), "|");
            }
        }

        public bool IsMember(int userId)
        {
            return Users.Any(uug => uug.UserId == userId);
        }

        /// <summary>
        /// Gets the preferred email addresses for a usergroup. 
        /// If the usergroup has an email address specified, this will return that email address. 
        /// Otherwise it will return email addresses of all users in the group.
        /// </summary>
        /// <returns>collection of email addresses</returns>
        public List<string> PreferredEmailAddresses(int idToExclude)
        {
            List<string> addresses = new List<string>();

            if (Utility.IsValidEmailAddress(GroupEmail))
            {
                addresses.Add(GroupEmail);
            }
            else
            {
                addresses.AddRange(from user in Users
                    .Where(u => u.User.IsApproved && 
                                !u.User.IsLockedOut && 
                                u.UserId != idToExclude && 
                                Utility.IsValidEmailAddress(u.User.Email)) where user.User.Email != null select user.User.Email);
            }
            return addresses;
        }
    }    
}