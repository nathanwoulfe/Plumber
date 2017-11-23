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
                if (null != model && model.Any())
                {
                    KeyValuePair<int, List<UserGroupPermissionsPoco>> permission = model.First();

                    _db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE NodeId = @0", permission.Key);

                    if (permission.Value.Any())
                    {
                        _db.BulkInsertRecords(permission.Value, DatabaseContext.SqlSyntax);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                var msg = "Error saving config. " + ex.Message;
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }

            return Ok();
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
                if (null != model)
                {
                    // set defaults for doctype - delete all previous if any model data exists
                    _db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE ContentTypeId != 0");

                    if (model.Any())
                    {
                        foreach (var permission in model)
                        {
                            if (permission.Value.Any())
                            {
                                _db.BulkInsertRecords(permission.Value, DatabaseContext.SqlSyntax);
                            }
                        }
                    }
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
