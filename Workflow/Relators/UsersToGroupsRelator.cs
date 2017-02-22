using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Relators
{
    public class UsersToGroupsRelator
    {
        public UserGroupPoco current;

        public UserGroupPoco MapIt(UserGroupPoco a, User2UserGroupPoco b)
        {
            if (a != null && current != null && current.GroupId == b.GroupId)
            {
                current.Users.Add(b);
                return null;
            }

            current = a;
            var prev = current;

            if (a == null)
            {
                return b == null ? prev : current;
            }

            if (current.GroupId == b.GroupId)
            {
                current.Users.Add(b);
            }

            return prev;
        }
    }
}
