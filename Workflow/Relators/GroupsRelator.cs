using System.Collections.Generic;
using System.Linq;
using Workflow.Models;

namespace Workflow.Relators
{
    public class GroupsRelator
    {
        private UserGroupPoco _current;

        public UserGroupPoco MapIt(UserGroupPoco a, UserGroupPermissionsPoco c, User2UserGroupPoco b)
        {
            if (a == null)
            {
                return _current;
            }

            if (_current != null && _current.GroupId == b.GroupId)
            {
                if (_current.Users.All(u => u.UserId != b.UserId))
                {
                    _current.Users.Add(b);
                }

                if (_current.Permissions.All(p => p.Id != c.Id))
                {
                    _current.Permissions.Add(c);
                }
                return null;
            }

            var prev = _current;
            _current = a;

            if (_current.GroupId == b.GroupId)
            {
                _current.Users = new List<User2UserGroupPoco>() { b };
            }

            if (_current.GroupId == c.GroupId)
            {
                _current.Permissions = new List<UserGroupPermissionsPoco>() { c };
            }

            return prev;
        }
    }
}
