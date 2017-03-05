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
        private static Database db = ApplicationContext.Current.DatabaseContext.Database;
        private static PocoRepository _pr = new PocoRepository();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetSettings()
        {
            return Request.CreateResponse(new
            {
                status = HttpStatusCode.OK,
                data = _pr.GetSettings()
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
                return Request.CreateResponse(HttpStatusCode.OK, new {
                    status = 0,
                    data = "This node is currently in a workflow"
                });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new {
                status = 200
            });
        }        
    }
}
