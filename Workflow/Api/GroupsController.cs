using log4net;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;
using Workflow;
using Workflow.Models;
using Workflow.Relators;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/groups")]
    public class GroupsController : UmbracoAuthorizedApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Database db = ApplicationContext.Current.DatabaseContext.Database;
        private static readonly PocoRepository _pr = new PocoRepository();

        /// <summary>
        /// Get group and associated users and permissions by id
        /// </summary>
        /// <param name="id">Optional, returns all groups if omitted</param>
        /// <returns></returns>       
        [Route("get/{id:int?}")]
        public IHttpActionResult Get(int? id = null)
        {
            try {
                if (id.HasValue)
                {
                    var result = _pr.PopulatedUserGroup(id.Value);

                    if (result.Any(r => !r.Deleted))
                    {
                        return Json(result.First(), ViewHelpers.CamelCase);
                    }
                }
                else
                {
                    var groups = _pr.UserGroups();
                    return Json(groups.Where(g => !g.Deleted), ViewHelpers.CamelCase);
                }

                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(e));
            }
        }

        /// <summary>
        /// Add a new group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]  
        [Route("add")]      
        public IHttpActionResult Post([FromBody]Model model)
        {
            var name = model.Data;

            try
            {                
                // check that it doesn't already exist
                if (_pr.UserGroupsByName(name).Any())
                {
                    return Ok(new { status = 500, msg = "Group name already exists" });
                }
                else
                {
                    // doesnt exist so create it with the given name. The alias will be generated from the name.
                    var newGroup = new UserGroupPoco
                    {
                        Name = name,
                        Alias = name.ToLower().Replace(" ", "-"),
                        Deleted = false
                    };

                    db.Insert(newGroup);
                }
            }
            catch (Exception ex)
            {
                var error = "Error creating user group '" + name + "'. " + ex.Message;
                log.Error(error, ex);
                // if we are here, something isn't right...
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, error));
            }

            var id = _pr.NewestGroup().GroupId;

            var msg = "Successfully created new user group '" + name + "'.";
            log.Debug(msg);

            // return the id of the new group, to update the front-end route to display the edit view
            return Ok(new { status = 200, msg = msg, id = id });
        }


        /// <summary>
        /// Save changes to an existing group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPut]
        [Route("save")]
        public IHttpActionResult Put(UserGroupPoco group)
        {
            var msgText = "";
            var nameExists = _pr.UserGroupsByName(group.Name).Any();
            var aliasExists = _pr.UserGroupsByAlias(group.Alias).Any();

            try
            {
                var userGroup = _pr.UserGroupsById(group.GroupId).First();

                // need to check the new name/alias isn't already in use
                if (userGroup.Name != group.Name && nameExists)
                {
                    return Content(HttpStatusCode.OK, new { status = 500, msg = "Group name already exists" });
                }

                if (userGroup.Alias != group.Alias && aliasExists)
                {
                    msgText = "Group alias already exists";
                    return Content(HttpStatusCode.OK, new { status = 500, msg = "Group alias already exists" });
                }

                // Update the Members - TODO - should find a more efficient way to do this...
                db.Execute("DELETE FROM WorkflowUser2UserGroup WHERE GroupId = @0", userGroup.GroupId);

                if (group.Users.Count > 0)
                {
                    foreach (var user in group.Users)
                    {
                        db.Insert(user);
                    }
                }

                db.Update(group);

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex));
            }

            // feedback to the browser
            msgText = "User group '" + group.Name + "' has been saved.";
            log.Debug(msgText);

            return Ok(new { status = 200, msg = msgText });
        }

        /// <summary>
        /// Delete group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpDelete]
        [Route("delete/{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            // remove all users, permissions and the group itself
            // existing workflow processes are left as is, and need to be managed by a human person
            try
            {
                db.Execute("UPDATE WorkflowUserGroups SET Deleted = 1 WHERE GroupId = @0", id);

                //db.Execute("DELETE FROM WorkflowUser2UserGroup WHERE GroupId = @0", id);
                //db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE GroupId = @0", id);
            }
            catch (Exception ex)
            {
                log.Error("Error deleting user group. " + ex.Message, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, "Error deleting user group"));
            }

            // gone.
            return Ok("User group has been deleted");
        }
    }
}