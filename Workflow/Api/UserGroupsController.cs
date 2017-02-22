using log4net;
using System;
using System.Collections.Generic;
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
        private IUserService _us = ApplicationContext.Current.Services.UserService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetAllGroups()
        {
            var userGroups = _pr.UserGroups();

            foreach (var userGroup in userGroups)
            {
                if (!userGroup.Name.Contains("Deleted"))
                {
                    var permissions = _pr.PermissionsForGroup(userGroup.GroupId);
                    if (permissions.Any())
                    {
                        userGroup.Permissions = permissions;
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, userGroups);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetGroup(string id)
        {
            var result = db.Fetch<UserGroupPoco, User2UserGroupPoco, UserGroupPoco>(
                new UserToGroupRelator().MapIt,
                @"SELECT * FROM WorkflowUserGroups LEFT OUTER JOIN WorkflowUser2UserGroup
                        on WorkflowUserGroups.GroupId = WorkflowUser2UserGroup.GroupId
                        LEFT OUTER JOIN WorkflowUserGroupPermissions
                        on WorkflowUserGroups.GroupId = WorkflowUserGroupPermissions.GroupId 
                        WHERE WorkflowUserGroups.GroupId = @0"
                , id);
            var permissions = _pr.PermissionsForGroup(int.Parse(id));

            if (result.Any())
            {
                var userGroup = result.First();

                if (permissions.Any())
                {
                    userGroup.Permissions = permissions;
                }

                var usersSummary = new List<int>();
                if (userGroup.Users.Any())
                {
                    var i = 0;
                    var remove = -1;
                    foreach (var u in userGroup.Users)
                    {
                        if (u.GroupId == 0 && u.UserId == 0 && u.Id == 0)
                        {
                            remove = i;
                        }
                        else
                        {
                            usersSummary.Add(u.UserId);                         
                        }
                        i++;
                    }
                    if (remove != -1)
                    {
                        userGroup.Users.Remove(userGroup.Users[remove]);
                    }
                    userGroup.UsersSummary = string.Concat("|", string.Join("|", usersSummary), "|");
                }

                return Request.CreateResponse(HttpStatusCode.OK, userGroup);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "Group not found");
        }

        /// <summary>
        /// 
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
                    return Request.CreateResponse(HttpStatusCode.NoContent, "Cannot create user group; a group with that name already exists.");
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

                return Request.CreateResponse(HttpStatusCode.NoContent, error);
            }

            var id = _pr.NewestGroup().GroupId;

            var msg = "Successfully created new user group '" + name + "'.";
            log.Debug(msg);

            return Request.CreateResponse(new {
                status = HttpStatusCode.OK,
                data = id
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage SaveGroup(UserGroupPoco ug)
        {
            var msgText = "";
            var nameExists = _pr.UserGroupsByName(ug.Name).Any();
            var aliasExists = _pr.UserGroupsByAlias(ug.Alias).Any();

            try
            {
                var userGroup = _pr.UserGroupsById(ug.GroupId.ToString()).First();

                if (userGroup.Name != ug.Name && nameExists)
                    return Request.CreateResponse(HttpStatusCode.NoContent, "Group name already exists");

                if (userGroup.Alias != ug.Alias && aliasExists)
                    return Request.CreateResponse(HttpStatusCode.NoContent, "Group alias already exists");

                // Update the Members

                db.Execute("DELETE FROM WorkflowUser2UserGroup WHERE GroupId = @0", userGroup.GroupId);

                if (ug.Users.Count > 0)
                {
                    foreach (var user in ug.Users)
                    {
                        db.Insert(user);
                    }
                }

                db.Update(userGroup);

            }
            catch (Exception ex)
            {
                msgText = "Error saving user group '" + ug.Name + "'. " + ex.Message;
                log.Error(msgText, ex);
                return Request.CreateResponse(HttpStatusCode.NoContent, msgText);
            }

            msgText = "User group '" + ug.Name + "' has been saved.";
            log.Debug(msgText);

            return Request.CreateResponse(new
            {
                status = HttpStatusCode.OK,
                data = msgText
            });
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage DeleteGroup(string id)
        {
            var name = db.Fetch<UserGroupPoco>("SELECT * FROM WorkflowUserGroups WHERE GroupId = @0", id).First().Name;
            
            try
            {
                db.Execute("DELETE FROM WorkflowUserGroups WHERE GroupId = @0", id);
                db.Execute("DELETE FROM WorkflowUser2UserGroup WHERE GroupId = @0", id);
                db.Execute("DELETE FROM WorkflowUserGroupPermissions WHERE GroupId = @0", id);
            }
            catch (Exception ex)
            {
                var error = "Error deleting UserGroup '" + name + "'. " + ex.Message;
                log.Error(error, ex);
                return Request.CreateResponse(HttpStatusCode.NoContent, error);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "User group '" + name + "' has been deleted");
        }
    }
}