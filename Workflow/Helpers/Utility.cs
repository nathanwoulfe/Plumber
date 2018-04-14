using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web;
using Workflow.Extensions;

namespace Workflow.Helpers
{
    public class Utility
    {
        private readonly UmbracoContext _context;
        private readonly UmbracoHelper _helper;

        private readonly IUserService _userService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IContentService _contentService;

        public Utility() : this(ApplicationContext.Current, UmbracoContext.Current)
        {
        }
        
        private Utility(ApplicationContext current, UmbracoContext context)
        {
            _context = context;
            _helper = new UmbracoHelper(_context);

            _userService = current.Services.UserService;
            _contentTypeService = current.Services.ContentTypeService;
            _contentService = current.Services.ContentService;
        }

        /// <summary>
        /// Get the node from cache, falling back to the db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IPublishedContent GetNode(int id)
        {
            IPublishedContent n = _helper.TypedContent(id);
            if (n != null) return n;

            IContent c = _contentService.GetById(id);

            return c?.ToPublishedContent();
        }

        /// <summary>
        /// Get the node name from cache, falling back to the db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetNodeName(int id)
        {
            IPublishedContent n = _helper.TypedContent(id);
            if (n != null) return n.Name;

            IContent c = _contentService.GetById(id);
            return c != null ? c.Name : MagicStrings.NoNode;
        }

        /// <summary>
        /// Get the  id of the root ancestor node for the given id
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public string GetRootNodeId(int nodeId)
        {
            return _contentService.GetById(nodeId).Path.Split(',')[1];
        }

        /// <summary>
        /// Get the user represented by the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IUser GetUser(int id)
        {
            return _userService.GetUserById(id);
        }

        /// <summary>
        /// Get the content type represented by the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IContentType GetContentType(int id)
        {
            return _contentTypeService.GetContentType(id);
        }

        /// <summary>
        /// Get the current logged-in user
        /// </summary>
        /// <returns></returns>
        public IUser GetCurrentUser()
        {
            return _context.Security.CurrentUser;
        }

        /// <summary>
        /// Convert a pascal-cased string to title case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string PascalCaseToTitleCase(string str)
        {
            return str != null ? Regex.Replace(str, "([A-Z]+?(?=(([A-Z]?[a-z])|$))|[0-9]+)", " $1").Trim() : null;
        }
        
        /// <summary>Checks whether the email address is valid.</summary>
        /// <param name="email">the email address to check</param>
        /// <returns>true if valid, false otherwise.</returns>
        public bool IsValidEmailAddress(string email)
        {
            try
            {
                var m = new MailAddress(email);
                return m.Address == email;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
