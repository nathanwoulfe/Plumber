using System.Collections.Generic;
using System.Linq;
using Workflow.Models;

namespace Workflow.Relators
{
    public class GroupsRelator
    {
        public UserGroupPoco Current;

        public UserGroupPoco MapIt(UserGroupPoco a, UserGroupPermissionsPoco c, User2UserGroupPoco b)
        {
            if (a == null)
            {
                return Current;
            }

            if (Current != null && Current.GroupId == b.GroupId)
            {
                if (Current.Users.All(u => u.UserId != b.UserId))
                {
                    Current.Users.Add(b);
                }

                if (Current.Permissions.All(p => p.Id != c.Id) && c.NodeName != MagicStrings.NoNode)
                {
                    Current.Permissions.Add(c);
                }
                return null;
            }

            var prev = Current;
            Current = a;

            if (Current.GroupId == b.GroupId)
            {
                Current.Users = new List<User2UserGroupPoco>() { b };
            }

            if (Current.GroupId == c.GroupId)
            {
                Current.Permissions = new List<UserGroupPermissionsPoco>() { c };
            }

            return prev;
        }
    }
}
