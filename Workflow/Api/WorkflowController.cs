using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;
using Workflow.Models;

namespace Workflow.Api
{
    public class WorkflowController : UmbracoAuthorizedApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Database db = ApplicationContext.Current.DatabaseContext.Database;
        private static PocoRepository _pr = new PocoRepository();
        private IUserService _us = ApplicationContext.Current.Services.UserService;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetSettings()
        {
            var settings = _pr.GetSettings();
            return Request.CreateResponse(new
            {
                status = HttpStatusCode.OK,
                data = settings
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage SaveSettings(WorkflowSettingsPoco model)
        {
            db.Update(model);            

            return Request.CreateResponse(new {
                status = HttpStatusCode.OK, 
                data = "Settings updated"
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetStatus(int nodeId)
        {
            var instances = _pr.InstancesByNodeAndStatus(nodeId, new List<int> { (int)WorkflowStatus.PendingApproval });

            if (instances.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { msg = "This node is currently in a workflow", status = 0 });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { msg = string.Empty, status = 1 });
        }        
    }
}
