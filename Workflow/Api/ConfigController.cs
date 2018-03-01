using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/config")]
    public class ConfigController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConfigService _configService;

        public ConfigController()
        {
            _configService = new ConfigService();
        }

        /// <summary>
        /// Persist the workflow approval config for single node
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("saveconfig")]
        public async Task<IHttpActionResult> SaveConfig(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            try
            {
                bool success = await _configService.UpdateNodeConfigAsync(model);
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
        public async Task<IHttpActionResult> SaveDocTypeConfig(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            try
            {
                bool success = await _configService.UpdateContentTypeConfigAsync(model);
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
