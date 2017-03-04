using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

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
        public MailAddressCollection PreferredEmailAddresses()
        {
            MailAddressCollection addresses = new MailAddressCollection();

            if (Helpers.IsValidEmailAddress(GroupEmail))
            {
                addresses.Add(new MailAddress(GroupEmail));
            }
            else
            {
                foreach (var user in Users)
                {
                    if (user.User.IsApproved && !user.User.IsLockedOut && Helpers.IsValidEmailAddress(user.User.Email))
                    {
                        addresses.Add(new MailAddress(user.User.Email));
                    }
                }
            }
            return addresses;
        }
   
        /// <summary>
        /// Delete this user group by setting its deleted flag in the database and renaming / realiasing it.
        /// </summary>
        //public void Delete()
        //{
        //    Deleted = true;
        //    Name += "_Deleted_" + DateTime.Now.ToString("ddMMyy");
        //    Alias += "_Deleted_" + DateTime.Now.ToString("ddMMyy");
        //    Users.Clear();
        //    //Permissions.Clear();
        //}

        /// <summary>
        /// Sets the users for the user group, based on the supplied id string
        /// </summary>
        /// <param name="userIds">comma separated string of user ids defining the group's users.</param>
        //public void SetUsers(string userIds)
        //{
        //    Users.Clear();
        //    foreach (var id in userIds.Split(','))
        //    {
        //        Users.Add(new User2UserGroupPoco { GroupId = Id, UserId = int.Parse(id) });
        //    }
        //}

    }    
}