using System;
using System.Linq;
using System.Web.Http;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/test")]
    public class DodgyTestsController : UmbracoAuthorizedApiController
    {
        private static readonly PocoRepository Pr = new PocoRepository();

        /// <summary>
        /// Endpoint for generating system notifications - sends email to pickup location defined in web.config 
        /// </summary>
        /// <param name="instanceId">The id of the workflow instance to notify</param>
        /// <param name="emailType">EmailType</param>
        /// <returns></returns>
        [HttpGet]
        [Route("notifications/{instanceId:int}/{emailType:int}")]
        public string GetNotifications(int instanceId, int emailType)
        {
            try
            {
                WorkflowInstancePoco instance = Pr.GetAllInstances().FirstOrDefault(x => x.Id == instanceId);

                Notifications.Send(instance, (EmailType)emailType);

                return "done - check the mail drop folder";
            }
            catch (Exception e)
            {
                return ViewHelpers.ApiException(e).ToString();
            }
        }
    }
}
