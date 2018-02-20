using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using log4net;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Workflow.Models;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/config")]
    public class ConfigController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                UmbracoDatabase db = DatabaseContext.Database;
                if (null != model && model.Any())
                {
                    KeyValuePair<int, List<UserGroupPermissionsPoco>> permission = model.First();

                    db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE NodeId = @0", permission.Key);

                    if (permission.Value.Any())
                    {
                        db.BulkInsertRecords(permission.Value, DatabaseContext.SqlSyntax);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                const string msg = "Error saving config";
                Log.Error(msg, ex);

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
                    UmbracoDatabase db = DatabaseContext.Database;

                    // set defaults for doctype - delete all previous if any model data exists
                    db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE ContentTypeId != 0");

                    if (model.Any())
                    {
                        foreach (KeyValuePair<int, List<UserGroupPermissionsPoco>> permission in model)
                        {
                            if (permission.Value.Any())
                            {
                                db.BulkInsertRecords(permission.Value, DatabaseContext.SqlSyntax);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                const string msg = "Error saving doctype config";
                Log.Error(msg, ex);

                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }

            return Ok();
        }
    }
}
