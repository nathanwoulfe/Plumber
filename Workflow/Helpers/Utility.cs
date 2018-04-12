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
    public static class Utility
    {
        private static readonly UmbracoHelper Helper = new UmbracoHelper(UmbracoContext.Current);
        private static readonly IUserService UserService = ApplicationContext.Current.Services.UserService;
        private static readonly IContentTypeService ContentTypeService = ApplicationContext.Current.Services.ContentTypeService;
        private static readonly IContentService ContentService = ApplicationContext.Current.Services.ContentService;

        /// <summary>
        /// Get the node from cache, falling back to the db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IPublishedContent GetNode(int id)
        {
            IPublishedContent n = Helper.TypedContent(id);
            if (n != null) return n;

            IContent c = ContentService.GetById(id);

            return c?.ToPublishedContent();
        }

        /// <summary>
        /// Get the node name from cache, falling back to the db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetNodeName(int id)
        {
            IPublishedContent n = Helper.TypedContent(id);
            if (n != null) return n.Name;

            IContent c = ContentService.GetById(id);
            return c != null ? c.Name : MagicStrings.NoNode;
        }

        /// <summary>
        /// Get the user represented by the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IUser GetUser(int id)
        {
            return UserService.GetUserById(id);
        }

        /// <summary>
        /// Get the content type represented by the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IContentType GetContentType(int id)
        {
            return ContentTypeService.GetContentType(id);
        }

        /// <summary>
        /// Get the current logged-in user
        /// </summary>
        /// <returns></returns>
        public static IUser GetCurrentUser()
        {
            return UmbracoContext.Current?.Security.CurrentUser;
        }

        /// <summary>
        /// Convert a pascal-cased string to title case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string PascalCaseToTitleCase(string str)
        {
            return str != null ? Regex.Replace(str, "([A-Z]+?(?=(([A-Z]?[a-z])|$))|[0-9]+)", " $1").Trim() : null;
        }
        
        /// <summary>Checks whether the email address is valid.</summary>
        /// <param name="email">the email address to check</param>
        /// <returns>true if valid, false otherwise.</returns>
        public static bool IsValidEmailAddress(string email)
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
