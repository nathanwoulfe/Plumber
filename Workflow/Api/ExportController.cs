using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;

namespace Workflow.Api
{
    /// <summary>
    /// Provides an endpoint for exporting the current workflow configuration
    /// </summary>
    [RoutePrefix("umbraco/backoffice/api/workflow/export")]
    public class ExportController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IImportExportService _exportService;

        public ExportController()
        {
            _exportService = new ImportExportService();
        }

        /// <summary>
        /// Get an object representing the end-to-end workflow configuration for the current environment. Great for importing somewhere else...
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                ImportExportModel export = await _exportService.Export();
                return Json(export, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                const string error = "Error exporting workflow configuration";
                Log.Error(error, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, error));
            }
        }
    }
}
