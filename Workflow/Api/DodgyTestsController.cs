using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/test")]
    public class DodgyTestsController : UmbracoAuthorizedApiController
    {
        private readonly IInstancesService _instancesService;

        public DodgyTestsController()
        {
            _instancesService = new InstancesService();
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
        public string GetNotifications(Guid instanceGuid, int emailType)
        {
            try
            {
                WorkflowInstancePoco instance = _instancesService.GetByGuid(instanceGuid);

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
