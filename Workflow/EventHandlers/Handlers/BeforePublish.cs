using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Workflow.Models;
using Workflow.Repositories;

namespace Workflow.EventHandlers.Handlers
{
    public class BeforePublish : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext context)
        {
            ContentService.Publishing += ContentService_Publishing;
        }

        private static void ContentService_Publishing(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            IContent doc = e.PublishedEntities.First();

            // we might want to cancel the publish event if it came from a scheduled publish
            // we can check if the node has an active workflow, in which case it should not publish, and it's likely from the scheduler
            // if the publish is scheduled, there is no httpContext

            try
            {
                // if a context exists, sweet, let it go
                if (null != HttpContext.Current) return;

                // ensure we have http context for queries
                HttpContext httpContext = new HttpContext(
                                              new HttpRequest(string.Empty, "http://tempuri.org", string.Empty),
                                              new HttpResponse(new StringWriter()));

                HttpContextBase httpContextBase = new HttpContextWrapper(httpContext);

                UmbracoContext.EnsureContext(
                    httpContextBase,
                    ApplicationContext.Current,
                    new WebSecurity(httpContextBase, ApplicationContext.Current),
                    UmbracoConfig.For.UmbracoSettings(),
                    UrlProviderResolver.Current.Providers,
                    false);

                var pr = new PocoRepository();
                List<WorkflowInstancePoco> instances = pr.InstancesByNodeAndStatus(doc.Id, new List<int>
                    {
                        (int)WorkflowStatus.PendingApproval,
                        (int)WorkflowStatus.Rejected,
                        (int)WorkflowStatus.Resubmitted
                    })
                    .OrderByDescending(i => i.CompletedDate).ToList();

                // if any incomplete workflows exists, cancel the publish
                // this will clear the release date, which is ok as it has passed
                // and the change will be released when the workflow completes
                if (instances.Any())
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
    }
}
