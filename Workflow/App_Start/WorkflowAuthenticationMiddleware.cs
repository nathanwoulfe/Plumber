using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Workflow.Helpers;

namespace Workflow
{
    internal class WorkflowAuthenticationMiddleware : OwinMiddleware
    {
        private UserData _userData;

        public WorkflowAuthenticationMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            IOwinRequest request = context.Request;
            if (request.Uri.AbsolutePath.StartsWith(MagicStrings.PreviewRouteBase) && request.Uri.Segments.Length == 6) 
            {
                string[] segments = request.Uri.Segments;

                string userId = segments[3].Trim('/');
                IUser user = ApplicationContext.Current.Services.UserService.GetUserById(int.Parse(userId));

                UserData userData = GetUserData(user);

                HttpContext.Current.Request.Cookies.Remove(UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName);

                HttpCookie authCookie = CreateAuthCookie(
                    user.Name,
                    segments[2].Trim('/'),
                    JsonConvert.SerializeObject(userData), 
                    GlobalSettings.TimeOutInMinutes,
                    UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName,
                    UmbracoConfig.For.UmbracoSettings().Security.AuthCookieDomain);

                HttpContext.Current.Request.Cookies.Add(authCookie);

                var identity = new UmbracoBackOfficeIdentity(userData);

                var securityHelper = new SecurityHelper(context);
                securityHelper.AddUserIdentity(identity);
                
            }

            if (Next != null)
            {
                await Next.Invoke(context);
            }
        }

        private UserData GetUserData(IUser user)
        {
            return new UserData(Guid.NewGuid().ToString())
            {
                Username = user.Username,
                Id = user.Id,
                AllowedApplications = user.AllowedSections.ToArray(),
                Culture = user.Language,
                RealName = user.Name,
                Roles = user.Groups.Select(g => g.Alias).ToArray(),
                StartContentNodes = user.StartContentIds,
                StartMediaNodes = user.StartMediaIds,
                SecurityStamp = user.SecurityStamp
            };
        }

        //borrowed from Umbraco - see source for code comments in CreateAuthTicketAndCookie
        private static HttpCookie CreateAuthCookie(string username, string nodeId, string userData,
            int loginTimeoutMins, string cookieName, string cookieDomain)
        {
            var ticket = new FormsAuthenticationTicket(4, username, DateTime.Now,
                DateTime.Now.AddMinutes(loginTimeoutMins), true, userData, $"/{nodeId}");

            string hash = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(cookieName, hash)
            {
                Expires = DateTime.Now.AddMinutes(loginTimeoutMins),
                Domain = cookieDomain,
                Path = $"/{nodeId}"
            };

            if (GlobalSettings.UseSSL)
                cookie.Secure = true;

            cookie.HttpOnly = true;
            return cookie;
        }
    }
}
