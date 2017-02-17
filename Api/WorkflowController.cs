using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
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
            var jsonFile = HttpContext.Current.Server.MapPath("/App_plugins/workflow/backoffice/workflow/settings.json");
            var settings = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(jsonFile));

            return Request.CreateResponse(HttpStatusCode.OK, settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage SaveSettings(SettingsModel model)
        {
            var jsonFile = HttpContext.Current.Server.MapPath("/App_plugins/workflow/backoffice/workflow/settings.json");
            var json = JsonConvert.SerializeObject(model);

            File.WriteAllText(jsonFile, json);

            return Request.CreateResponse(HttpStatusCode.OK, "Settings updated");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetStatus(int nodeId)
        {
            var instances = _pr.InstancesByNodeAndStatus(nodeId, new List<int> { (int)WorkflowStatus.PendingCoordinatorApproval, (int)WorkflowStatus.PendingFinalApproval });

            if (instances.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { msg = "This node is currently in a workflow", status = 0 });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { msg = string.Empty, status = 1 });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="authorId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage InitiateWorkflow(string nodeId, string comment, bool publish)
        {
            WorkflowInstancePoco instance = null;
            TwoStepApprovalProcess process = null;

            try
            {
                if (publish)
                {
                    process = new DocumentPublishProcess();
                }
                else
                {
                    process = null;
                }

                instance = process.InitiateWorkflow(int.Parse(nodeId), Helpers.GetCurrentUser().Id, comment);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Something went wrong " + e.Message );
            }

            if (instance != null)
            {
                var msg = string.Empty;

                switch (instance._Status)
                {
                    case WorkflowStatus.PendingCoordinatorApproval:
                        msg = "Page submitted for coordinator approval";
                        break;
                    case WorkflowStatus.PendingFinalApproval:
                        msg = "Page submitted for final approval";
                        break;
                    case WorkflowStatus.Completed:
                        msg = "Workflow complete";
                        break;
                }
                return Request.CreateResponse(HttpStatusCode.OK, msg);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "Something went wrong");
        }
    }
}
