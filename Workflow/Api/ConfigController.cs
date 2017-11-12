using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/config")]
    public class ConfigController : UmbracoAuthorizedApiController
    {
        private readonly Database _db = ApplicationContext.Current.DatabaseContext.Database;

        /// <summary>
        /// Persist the workflow approval config
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("save")]
        public IHttpActionResult Save(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            try
            {
                var data = model.First();

                if (data.Value.Any(p => p.ContentTypeId > 0))
                {
                    // set defaults for doctype - delete all previous
                    _db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE ContentTypeId != 0");
                }
                else
                {
                    _db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE NodeId = @0", data.Key);
                }

                if (data.Value.Any())
                {
                    _db.BulkInsertRecords(data.Value, DatabaseContext.SqlSyntax);
                }
            }
            catch (Exception ex)
            {
                var msg = "Error saving config. " + ex.Message;
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }

            return Ok();
        }
    }
}
