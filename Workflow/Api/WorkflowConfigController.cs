using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;
using Workflow.Models;

namespace Workflow.Api
{
    public class WorkflowConfigController : UmbracoAuthorizedApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Database db = ApplicationContext.Current.DatabaseContext.Database;

        /// <summary>
        /// Persist the workflow approval config
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public IHttpActionResult SaveConfig(List<UserGroupPermissionsPoco> model)
        {
            try
            {
                if (model.Where(p => p.ContentTypeId > 0).Any())
                {
                    // set defaults for doctype - delete all previous
                    db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE ContentTypeId != 0");
                    model.Where(x => x.Id == 0).ForEach(c => db.Insert(c));
                    model.Where(x => x.Id > 0).ForEach(c => db.Update(c));    
                }
                else
                {
                    foreach (var m in model)
                    {
                        // if an id exists, update existing
                        if (m.Id > 0)
                        {
                            db.Update(m);
                        }
                        else
                        {
                            var exists = db.Fetch<UserGroupPermissionsPoco>(@"SELECT * FROM WorkflowUserGroupPermissions 
                                                                    WHERE GroupId = @0 
                                                                    AND NodeId = @1 
                                                                    AND Permission = @2", m.GroupId, m.NodeId, 0);
                            if (exists.Any())
                            {
                                var p = exists.First();
                                p.Permission = m.Permission;
                                db.Update(p);
                            }
                            else
                            {
                                db.Insert(m);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "Error saving config. " + ex.Message;
                log.Error(msg, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, msg));
            }

            return Ok();
        }
    }
}
