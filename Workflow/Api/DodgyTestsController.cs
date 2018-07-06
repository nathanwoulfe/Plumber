using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Utility _utility;

        public DodgyTestsController()
        {
            _instancesService = new InstancesService();
            _emailer = new Emailer();
            _utility = new Utility();
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

                var node = _utility.GetPublishedContent(1078);

                _emailer.Send(instance, (EmailType)emailType);

                return node.Name;
            }
            catch (Exception e)
            {
                return ViewHelpers.ApiException(e).ToString();
            }
        }
    }
}
