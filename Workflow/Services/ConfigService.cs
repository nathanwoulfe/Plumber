using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Workflow.Events.Args;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services.Interfaces;

namespace Workflow.Services
{
    /// <summary>
    /// Service for persisting changes to workflow config on nodes or content types
    /// </summary>
    public class ConfigService : IConfigService
    {
        private readonly ILogger _log;
        private readonly IPocoRepository _repo;

        public static event EventHandler ConfigUpdated;
        protected virtual void OnConfigUpdated(EventArgs e)
        {
            ConfigUpdated?.Invoke(this, e);
        }

        public ConfigService()
            : this(
                  ApplicationContext.Current.ProfilingLogger.Logger,
                  new PocoRepository(ApplicationContext.Current.DatabaseContext.Database)
            )
        {
        }

        public ConfigService(ILogger log, IPocoRepository repo)
        {
            _log = log;
            _repo = repo;
        }

        /// <summary>
        /// Update the stored workflow config
        /// </summary>
        /// <param name="model">Dictionary representing the indexed permissions for the node</param>
        /// <returns>Bool representing success state</returns>
        public bool UpdateNodeConfig(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            if (null == model || !model.Any()) return false;

            KeyValuePair<int, List<UserGroupPermissionsPoco>> permission = model.First();

            _repo.DeleteNodeConfig(permission.Key);

            if (!permission.Value.Any()) return false;

            foreach (UserGroupPermissionsPoco poco in permission.Value)
                _repo.AddPermission(poco);

            // emit event
            OnConfigUpdated(new OnConfigUpdatedEventArgs
            {
                Model = model,
                UpdatedBy = Utility.GetCurrentUser()
            });

            return true;
        }

        /// <summary>
        /// Update the stored workflow config for all content types
        /// </summary>
        /// <param name="model">Dictionary representing the indexed permissions for the content type</param>
        /// <returns>Bool representing success state</returns>
        public bool UpdateContentTypeConfig(Dictionary<int, List<UserGroupPermissionsPoco>> model)
        {
            if (null == model || !model.Any()) return false;

            _repo.DeleteContentTypeConfig();           
                
            foreach (KeyValuePair<int, List<UserGroupPermissionsPoco>> permission in model)
            {
                if (!permission.Value.Any()) continue;
                foreach (UserGroupPermissionsPoco perm in permission.Value)
                {
                    _repo.AddPermission(perm);
                }
            }

            // emit event
            OnConfigUpdated(new OnConfigUpdatedEventArgs
            {
                Model = model,
                UpdatedBy = Utility.GetCurrentUser()
            });

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public bool HasFlow(int nodeId)
        {
            return _repo.HasFlow(nodeId);
        }

        /// <summary>
        /// Get the assigned permissions for the given node or content type id
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public List<UserGroupPermissionsPoco> GetPermissionsForNode(int nodeId, int? contentTypeId)
        {
            List<UserGroupPermissionsPoco> permissions = _repo.PermissionsForNode(nodeId, contentTypeId);

            return permissions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<UserGroupPermissionsPoco> GetRecursivePermissionsForNode(IPublishedContent node)
        {
            List<UserGroupPermissionsPoco> permissions = GetPermissionsForNode(node);
            return permissions;
        }

        /// <summary>
        /// Get the explicit or implied approval flow for a given node
        /// Will return explicit, content type, or inherited, in that order
        /// </summary>
        private List<UserGroupPermissionsPoco> GetPermissionsForNode(IPublishedContent node)
        {
            if (node == null) return null;
            int nodeId = node.Id;

            while (true)
            {
                // check the node for set permissions
                // return them if they exist, otherwise check for content type, then the parent if none set for the type
                List<UserGroupPermissionsPoco> permissions = _repo.PermissionsForNode(node.Id, 0);
                if (permissions.Any()) return permissions;

                if (nodeId == node.Id)
                {
                    permissions = _repo.PermissionsForNode(0, node.ContentType.Id);
                    if (permissions.Any()) return permissions;
                }

                if (node.Level > 1)
                {
                    node = node.Parent;
                }
            }
        }
    }
}
