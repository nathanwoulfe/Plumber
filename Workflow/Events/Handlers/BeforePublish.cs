using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using log4net;
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
using Workflow.Services;

namespace Workflow.Events.Handlers
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
            ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

                var instancesService = new InstancesService();
                IEnumerable<WorkflowInstancePoco> instances = instancesService.GetForNodeByStatus(doc.Id, new List<int>
                    {
                        (int)WorkflowStatus.PendingApproval,
                        (int)WorkflowStatus.Rejected,
                        (int)WorkflowStatus.Resubmitted
                    });
                    
                 List<WorkflowInstancePoco> orderedInstances = instances.OrderByDescending(i => i.CompletedDate).ToList();

                // if any incomplete workflows exists, cancel the publish
                // this will clear the release date, which is ok as it has passed
                // and the change will be released when the workflow completes
                if (!orderedInstances.Any()) return;

                e.Cancel = true;

                log.Info($"Scheduled publish for {doc.Name} cancelled due to active workflow");
            }
            catch (Exception ex)
            {
                log.Error($"Error in scheduled publish validation for {doc.Name}", ex);
            }
        }
    }
}
