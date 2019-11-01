using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Workflow.Extensions;

namespace Workflow.Models
{
    [TableName("WorkflowUserGroups")]
    [ExplicitColumns]
    [PrimaryKey("GroupId", autoIncrement = true)]
    public class UserGroupPoco
    {
        [Column("GroupId")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        [JsonProperty("groupId")]
        public int GroupId { get; set; }

        [Column("Description")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [JsonProperty("description")]
        public string Description { get; set; }

        [Column("Name")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Column("Alias")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [Column("GroupEmail")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [JsonProperty("groupEmail")]
        public string GroupEmail { get; set; }

        [Column("AdditionalGroupEmails")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [JsonProperty("additionalGroupEmails")]
        public string AdditionalGroupEmails { get; set; }

        [Column("OfflineApproval")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [JsonProperty("offlineApproval")]
        public bool OfflineApproval { get; set; }

        [Column("Deleted")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [ResultColumn]
        [JsonProperty("permissions")]
        public List<UserGroupPermissionsPoco> Permissions { get; set; }

        [ResultColumn]
        [JsonProperty("users")]
        public List<User2UserGroupPoco> Users { get; set; }

        [ResultColumn]
        [JsonProperty("usersSummary")]
        public string UsersSummary {
            get
            {
                return string.Concat("|", string.Join("|", Users.Select(u => u.UserId)), "|");
            }
        }

        public UserGroupPoco()
        {
            Users = new List<User2UserGroupPoco>();
            Permissions = new List<UserGroupPermissionsPoco>();
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
        public List<string> PreferredEmailAddresses()
        {
            List<string> addresses = new List<string>();

            if (GroupEmail.IsValidEmailAddress())
            {
                addresses.Add(GroupEmail);
            }
            else
            {
                addresses.AddRange(from user in Users
                    .Where(u => u.User.IsApproved && 
                                !u.User.IsLockedOut && 
                                u.User.Email.IsValidEmailAddress()) where user.User.Email != null select user.User.Email);
            }
            
            return addresses;
        }

        /// <summary>
        /// Gets the CC email addresses for a usergroup. 
        /// </summary>
        /// <returns>collection of email addresses</returns>
        public List<string> AdditionalEmailAddresses()
        {
            List<string> addresses = new List<string>();
            var emails = AdditionalGroupEmails.Split(',').Select(e => e.Trim()).ToList();

            foreach (var email in emails) {
                if (email.IsValidEmailAddress()) {
                    addresses.Add(email);
                }
            }
            return addresses;
        }
    }    
}