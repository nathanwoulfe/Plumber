//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Http;
//using Umbraco.Web.WebApi;
//using Workflow.Helpers;
//using Workflow.Models;
//using Workflow.Repositories;
//using Workflow.Services;

//namespace Workflow.Api
//{
//    [RoutePrefix("umbraco/backoffice/api/workflow/test")]
//    public class DodgyTestsController : UmbracoAuthorizedApiController
//    {
//        private readonly IInstancesService _instancesService;

//        public DodgyTestsController()
//        {
//            _instancesService = new InstancesService();
//        }

//        /// <summary>
//        /// Endpoint for generating system notifications - sends email to pickup location defined in web.config
//        /// Really dodgy. Simply a means to generate notifications for testing 
//        /// </summary>
//        /// <param name="instanceId">The id of the workflow instance to notify</param>
//        /// <param name="emailType">EmailType</param>
//        /// <returns></returns>
//        [HttpGet]
//        [Route("notifications/{instanceId:int}/{emailType:int}")]
//        public string GetNotifications(int instanceId, int emailType)
//        {
//            try
//            {
//                List<WorkflowInstancePoco> instances = _instancesService.Get();
//                WorkflowInstancePoco instance = instances.FirstOrDefault(x => x.Id == instanceId);

//                Notifications.Send(instance, (EmailType)emailType);

//                return "done - check the mail drop folder";
//            }
//            catch (Exception e)
//            {
//                return ViewHelpers.ApiException(e).ToString();
//            }
//        }
//    }
//}
