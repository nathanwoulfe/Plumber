using log4net;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;
using Workflow;
using Workflow.Models;
using Workflow.Relators;

namespace Usc.Web.UserGroups
{
    public class UserGroupsController : UmbracoAuthorizedApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Database db = ApplicationContext.Current.DatabaseContext.Database;
        private static readonly PocoRepository _pr = new PocoRepository();

        /// <summary>
        /// Get all groups and their associated users and permissions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetAllGroups()
        {
            return Request.CreateResponse(new
            {
                stats = HttpStatusCode.OK,
                data = _pr.UserGroups()
            });
        }

        /// <summary>
        /// Get single group and associated users and permissions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetGroup(string id)
        {
            var result = _pr.PopulatedUserGroup(id);

            if (result.Any())
            {
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.OK,
                    data = result.First()
                });
            }
            return Request.CreateResponse(new
            {
                status = HttpStatusCode.NotFound,
                data = "Group not found"
            });
        }

        /// <summary>
        /// Add a new group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage AddGroup(string name)
        {
            try
            {
                // check that it doesn't already exist
                if (_pr.UserGroupsByName(name).Any())
                {
                    return Request.CreateResponse(new
                    {
                        status = HttpStatusCode.NoContent,
                        data = "Cannot create user group; a group with that name already exists."
                    });
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
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.NoContent,
                    data = error
                });
            }

            var id = _pr.NewestGroup().GroupId;

            var msg = "Successfully created new user group '" + name + "'.";
            log.Debug(msg);

            // return the id of the new group, to update the front-end route to display the edit view
            return Request.CreateResponse(new {
                status = HttpStatusCode.OK,
                data = id
            });
        }


        /// <summary>
        /// Save changes to an existing group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage SaveGroup(UserGroupPoco group)
        {
            var msgText = "";
            var nameExists = _pr.UserGroupsByName(group.Name).Any();
            var aliasExists = _pr.UserGroupsByAlias(group.Alias).Any();

            try
            {
                var userGroup = _pr.UserGroupsById(group.GroupId.ToString()).First();

                // need to check the new name/alias isn't already in use
                if (userGroup.Name != group.Name && nameExists)
                    return Request.CreateResponse(new
                    {
                        status = HttpStatusCode.NoContent,
                        data = "Group name already exists"
                    });

                if (userGroup.Alias != group.Alias && aliasExists)
                    return Request.CreateResponse(new
                    {
                        status = HttpStatusCode.NoContent,
                        data = "Group alias already exists"
                    });

                // Update the Members - TODO - should find a more efficient way to do this...
                db.Execute("DELETE FROM WorkflowUser2UserGroup WHERE GroupId = @0", userGroup.GroupId);

                if (group.Users.Count > 0)
                {
                    foreach (var user in group.Users)
                    {
                        db.Insert(user);
                    }
                }

                db.Update(userGroup);

            }
            catch (Exception ex)
            {
                msgText = "Error saving user group '" + group.Name + "'. " + ex.Message;
                log.Error(msgText, ex);
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.NoContent,
                    data = msgText
                });
            }

            // feedback to the browser
            msgText = "User group '" + group.Name + "' has been saved.";
            log.Debug(msgText);

            return Request.CreateResponse(new
            {
                status = HttpStatusCode.OK,
                data = msgText
            });
        }

        /// <summary>
        /// Delete group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage DeleteGroup(string id)
        {
            // remove all users, permissions and the group itself
            // existing workflow processes are left as is, and need to be managed by a human person
            try
            {
                db.Execute("DELETE FROM WorkflowUserGroups WHERE GroupId = @0", id);
                db.Execute("DELETE FROM WorkflowUser2UserGroup WHERE GroupId = @0", id);
                db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE GroupId = @0", id);
            }
            catch (Exception ex)
            {
                var error = "Error deleting user group. " + ex.Message;
                log.Error(error, ex);
                return Request.CreateResponse(new
                {
                    status = HttpStatusCode.NoContent,
                    data = error
                });
            }

            // gone.
            return Request.CreateResponse(new
            {
                status = HttpStatusCode.OK,
                data = string.Concat("User group has been deleted")
            });
        }
    }
}