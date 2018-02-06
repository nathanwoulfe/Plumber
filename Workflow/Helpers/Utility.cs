using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web;
using Workflow.Extensions;
using Workflow.Models;

namespace Workflow.Helpers
{
    public class Utility
    {
        private static readonly UmbracoHelper Helper = new UmbracoHelper(UmbracoContext.Current);
        private static readonly IUserService Us = ApplicationContext.Current.Services.UserService;
        private static readonly IContentTypeService Cts = ApplicationContext.Current.Services.ContentTypeService;
        private static readonly IContentService Cs = ApplicationContext.Current.Services.ContentService;
        private static readonly PocoRepository Pr = new PocoRepository();

        public static IPublishedContent GetNode(int id)
        {
            var n = Helper.TypedContent(id);
            if (n != null) return n;

            var c = Cs.GetById(id);

            return c?.ToPublishedContent();
        }

        public static string GetNodeName(int id)
        {
            var n = Helper.TypedContent(id);
            if (n != null) return n.Name;

            var c = Cs.GetById(id);
            return c != null ? c.Name : MagicStrings.NoNode;
        }

        public static bool GetNodeStatus(int id)
        {
            return Pr.InstancesByNodeAndStatus(id, new List<int> { (int)WorkflowStatus.PendingApproval }).Any();
        }

        public static IUser GetUser(int id)
        {
            return Us.GetUserById(id);
        }

        public static IContentType GetContentType(int id)
        {
            return Cts.GetContentType(id);
        }

        public static IUser GetCurrentUser()
        {
            return UmbracoContext.Current.Security.CurrentUser;
        }

        public static bool IsTypeOfAdmin(string utAlias)
        {
            return utAlias == "admin" || utAlias == "siteadmin";
        }

        public static string PascalCaseToTitleCase(string str)
        {
            return str != null ? Regex.Replace(str, "([A-Z]+?(?=(([A-Z]?[a-z])|$))|[0-9]+)", " $1").Trim() : null;
        }

        public static WorkflowSettingsPoco GetSettings()
        {
            return Pr.GetSettings();
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
