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
        private readonly Notifications _notifications;
        private readonly Utility _utility;

        public DodgyTestsController()
        {
            _instancesService = new InstancesService();
            _notifications = new Notifications();
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

                var node = _utility.GetNode(1078);

                _notifications.Send(instance, (EmailType)emailType);

                return node.Name;
            }
            catch (Exception e)
            {
                return ViewHelpers.ApiException(e).ToString();
            }
        }
    }
}
