using System;
using System.Web;
using umbraco;
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
            // applicationUrl is null in tests, so can use it to avoid hitting the published cache, which is not available
            if(_context.Application.UmbracoApplicationUrl != null)
            {
                IPublishedContent n = _context.ContentCache.GetById(id);
                if (n != null) return n;
            }

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
        /// Get the node name from the db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetNodeName(int id)
        {
            IPublishedContent node = GetPublishedContent(id);
            return node != null ? node.Name : Constants.NoNode;
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
        /// Wrapper for IPublishedContent.GetPropertyValue<string>()
        /// Avoids Umbraco.Web dependency in calling class
        /// </summary>
        /// <param name="content"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public string GetPropertyValueAsString(IPublishedContent content, string alias)
        {
            return content.GetPropertyValue<string>(alias);
        }

        /// <summary>
        /// Get the current logged-in user
        /// </summary>
        /// <returns></returns>
        public IUser GetCurrentUser()
        {
            return _context.Security.CurrentUser;
        }

        /// <summary />
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="path"></param>
        /// <param name="httpOnly"></param>
        public static void SetCookie(string name, string value, string path = "/", bool httpOnly = true)
        {
            HttpContext context = HttpContext.Current;

            var cookie = new HttpCookie(name, value)
            {
                Expires = DateTime.Now.AddMinutes(GlobalSettings.TimeOutInMinutes),
                HttpOnly = httpOnly,
                Path = path
            };

            context.Response.Cookies.Set(cookie);

            cookie = context.Request.Cookies[name];

            if (cookie != null)
            {
                cookie.Value = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookieName"></param>
        public static void ExpireCookie(string cookieName)
        {
            HttpContext http = HttpContext.Current;

            //remove from the request
            http.Request.Cookies.Remove(cookieName);

            //expire from the response
            HttpCookie angularCookie = http.Response.Cookies[cookieName];
            if (angularCookie != null)
            {
                //this will expire immediately and be removed from the browser
                angularCookie.Expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                //ensure there's def an expired cookie
                http.Response.Cookies.Add(new HttpCookie(cookieName) { Expires = DateTime.Now.AddYears(-1) });
            }
        }
    }
}
