using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web;
using Workflow.Extensions;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;

namespace Workflow.Helpers
{
    public class Utility
    {
        private readonly UmbracoContext _context;
        private readonly UmbracoHelper _helper;

        private readonly IUserService _userService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IContentService _contentService;
        private readonly IPocoRepository _pocoRepo;

        public Utility() : this(
            new PocoRepository(), 
            ApplicationContext.Current.Services.UserService,
            ApplicationContext.Current.Services.ContentTypeService,
            ApplicationContext.Current.Services.ContentService,
            UmbracoContext.Current)
        {
        }
        
        public Utility(IPocoRepository pocoRepo, IUserService userService, IContentTypeService contentTypeService, IContentService contentService, UmbracoContext context)
        {
            _context = context;
            _helper = new UmbracoHelper(_context);

            _userService = userService;
            _contentTypeService = contentTypeService;
            _contentService = contentService;

            _pocoRepo = pocoRepo;
        }

        /// <summary>
        /// Get the node from cache, falling back to the db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IPublishedContent GetPublishedContent(int id)
        {
            IPublishedContent n = _helper.TypedContent(id);
            if (n != null) return n;

            IContent c = _contentService.GetById(id);

            return c?.ToPublishedContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IContent GetContent(int id)
        {
            return _contentService.GetById(id);
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
        public bool HasFlow(int nodeId)
        {
            string root = _contentService.GetById(nodeId).Path.Split(',')[1];
            return _pocoRepo.NodeHasPermissions(int.Parse(root));
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
