using Workflow.Models;

namespace Workflow.Relators
{
    internal class UserToGroupForPermissionsRelator
    {
        public UserGroupPermissionsPoco Current;

        /// <summary>
        /// Maps Users to the UserGroup property of a UserGroupPermissionsPoco
        /// </summary>
        /// <param name="ugpp"></param>
        /// <param name="ugp"></param>
        /// <param name="u2Ugp"></param>
        /// <returns></returns>
        public UserGroupPermissionsPoco MapIt(UserGroupPermissionsPoco ugpp, UserGroupPoco ugp, User2UserGroupPoco u2Ugp)
        {
            if (ugpp == null)
            {
                return Current;
            }

            if (Current != null && Current.GroupId == ugpp.GroupId)
            {
                if (Current.UserGroup == null)
                {
                    Current.UserGroup = ugp;
                } 
                
                if (Current.GroupId == u2Ugp.GroupId)
                {
                    Current.UserGroup.Users.Add(u2Ugp);
                }      
                return null;
            }

            var prev = Current;
            Current = ugpp;
            Current.UserGroup = ugp;

            if (Current.GroupId == u2Ugp.GroupId)
            {
                Current.UserGroup.Users.Add(u2Ugp);
            }

            return prev;
        }
    }
}