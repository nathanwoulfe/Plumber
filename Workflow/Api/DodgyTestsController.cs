using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Notifications;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/test")]
    public class DodgyTestsController : UmbracoAuthorizedApiController
    {
        private readonly IInstancesService _instancesService;
        private readonly Emailer _emailer;

        public DodgyTestsController()
        {
            _instancesService = new InstancesService();
            _emailer = new Emailer();
        }

        /// <summary>
        /// Endpoint for generating system notifications - sends email to pickup location defined in web.config
        /// Really dodgy. Simply a means to generate notifications for testing 
        /// </summary>
        /// <param name="instanceGuid"></param>
        /// <param name="emailType">EmailType</param>
        /// <returns></returns>
        [HttpGet]
        [Route("notifications/{instanceGuid:guid}/{emailType:int}")]
        public async Task<HttpResponseMessage> GetNotifications(Guid instanceGuid, int emailType)
        {
            try
            {
                WorkflowInstancePoco instance = _instancesService.GetByGuid(instanceGuid);

                var msg = await _emailer.Send(instance, (EmailType)emailType);

                var response = new HttpResponseMessage
                {
                    Content = new StringContent(msg)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

                return response;
            }
            catch (Exception e)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent(ViewHelpers.ApiException(e).ToString())
                };
            }
        }
    }
}
