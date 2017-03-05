using System.Collections.Generic;
using System.Linq;
using Workflow.Models;

namespace Workflow.Relators
{
    public class GroupsRelator
    {
        public UserGroupPoco current;

        public UserGroupPoco MapIt(UserGroupPoco a, UserGroupPermissionsPoco c, User2UserGroupPoco b)
        {
            if (a == null)
            {
                return current;
            }

            if (a != null && current != null && current.GroupId == b.GroupId)
            {
                if (!current.Users.Where(u => u.UserId == b.UserId).Any())
                {
                    current.Users.Add(b);
                }

                if (!current.Permissions.Where(p => p.Id == c.Id).Any())
                {
                    current.Permissions.Add(c);
                }
                return null;
            }

            var prev = current;
            current = a;

            if (current.GroupId == b.GroupId)
            {
                current.Users = new List<User2UserGroupPoco>() { b };
            }

            if (current.GroupId == c.GroupId)
            {
                current.Permissions = new List<UserGroupPermissionsPoco>() { c };
            }

            return prev;
        }
    }
}
