using System.Linq;
using Workflow.Models;

namespace Workflow
{
    internal class UserToGroupForPermissionsRelator
    {
        public UserGroupPermissionsPoco current;

        /// <summary>
        /// Maps Users to the UserGroup property of a UserGroupPermissionsPoco
        /// </summary>
        /// <param name="wtip"></param>
        /// <param name="wip"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public UserGroupPermissionsPoco MapIt(UserGroupPermissionsPoco ugpp, UserGroupPoco ugp, User2UserGroupPoco u2ugp)
        {
            if (ugpp == null)
            {
                return current;
            }

            //if (u2ugp.GroupId == ugp.GroupId)
            //{
            //    ugp.Users.Add(u2ugp);
            //}

            if (current != null && current.GroupId == ugpp.GroupId)
            {
                if (current.UserGroup == null)
                {
                    current.UserGroup = ugp;
                } 
                
                if (current.GroupId == u2ugp.GroupId)
                {
                    current.UserGroup.Users.Add(u2ugp);
                }      
                return null;
            }

            var prev = current;
            current = ugpp;
            current.UserGroup = ugp;

            if (current.GroupId == u2ugp.GroupId)
            {
                current.UserGroup.Users.Add(u2ugp);
            }

            return prev;
        }
    }
}