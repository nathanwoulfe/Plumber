using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Relators
{
    public class UserToGroupRelator
    {
        public UserGroupPoco current;

        public UserGroupPoco MapIt(UserGroupPoco a, User2UserGroupPoco b)
        {           
            if (a == null)
            {
                return current;
            }

            if (a != null  && current != null && a.GroupId == b.GroupId) {
                current.Users.Add(b);                
                return null;
            }

            var prev = current;
            current = a;
            current.Users = new List<User2UserGroupPoco>() { b };

            return prev;
        }
    }
}
