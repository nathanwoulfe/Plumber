using System.Collections.Generic;
using System.Linq;
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

            if (current != null && current.GroupId == a.GroupId) {
                if (!current.Users.Where(u => u.UserId == b.UserId).Any())
                {
                    current.Users.Add(b);
                }   
                return null;
            }

            var prev = current;
            current = a;
            current.Users = new List<User2UserGroupPoco>() { b };

            return prev;
        }
    }
}
