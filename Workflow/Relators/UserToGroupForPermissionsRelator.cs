using Workflow.Models;

namespace Workflow.Relators
{
    internal class UserToGroupForPermissionsRelator
    {
        private UserGroupPermissionsPoco _current;

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
                return _current;
            }

            if (_current != null && _current.GroupId == ugpp.GroupId)
            {
                if (_current.UserGroup == null)
                {
                    _current.UserGroup = ugp;
                } 
                
                if (_current.GroupId == u2Ugp.GroupId)
                {
                    _current.UserGroup.Users.Add(u2Ugp);
                }      
                return null;
            }

            var prev = _current;
            _current = ugpp;
            _current.UserGroup = ugp;

            if (_current.GroupId == u2Ugp.GroupId)
            {
                _current.UserGroup.Users.Add(u2Ugp);
            }

            return prev;
        }
    }
}