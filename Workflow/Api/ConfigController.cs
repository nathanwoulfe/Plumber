using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using log4net;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/config")]
    public class ConfigController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConfigService _configService;

        public ConfigController() : this(new ConfigService())
        {
        }

        public ConfigController(IConfigService configSerivce)
        {
            _configService = configSerivce;
        }

        /// <summary>
        /// Check root nodes have a group assigned
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("workflowconfigured")]
        public IHttpActionResult WorkflowConfigured()
        {
            IEnumerable<IPublishedContent> rootNodes = Umbraco.TypedContentAtRoot();
            List<string> response = new List<string>();

            foreach (IPublishedContent node in rootNodes)
            {
                List<UserGroupPermissionsPoco> permissions = _configService.GetPermissionsForNode(node.Id);
                if (!permissions.Any())
                {
                    response.Add(node.Name);
                }
            }

            return Json(response, ViewHelpers.CamelCase);
        }

        /// <summary>
        /// Persist the workflow approval config for single node
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("saveconfig")]
        public IHttpActionResult SaveConfig(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            try
            {
                bool success = _configService.UpdateNodeConfig(model);
                return Ok(success);
            }
            catch (Exception ex)
            {
                const string msg = "Error saving config";
                Log.Error(msg, ex);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }

        }

        /// <summary>
        /// Persist the workflow approval config for doctypes
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("savedoctypeconfig")]
        public IHttpActionResult SaveDocTypeConfig(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            try
            {
                bool success = _configService.UpdateContentTypeConfig(model);
                return Ok(success);
            }
            catch (Exception ex)
            {
                const string msg = "Error saving doctype config";
                Log.Error(msg, ex);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }
        }
    }
}
