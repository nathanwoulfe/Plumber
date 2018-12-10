using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Workflow.Events.Args;
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
        private readonly IPocoRepository _repo;

        public static event EventHandler<ConfigEventArgs> Updated;

        public ConfigService()
            : this(new PocoRepository())
        {
        }

        public ConfigService(IPocoRepository repo)
        {
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

            Updated?.Invoke(this, new ConfigEventArgs(model, "Node"));

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

            Updated?.Invoke(this, new ConfigEventArgs(model, "ContentType"));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<UserGroupPermissionsPoco> GetAll()
        {
            List<UserGroupPermissionsPoco> permissions = _repo.GetAllPermissions();

            return permissions;
        }

        /// <summary>
        /// Get the assigned permissions for the given node or content type id
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public List<UserGroupPermissionsPoco> GetPermissionsForNode(int nodeId, int contentTypeId = 0)
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
            if (node == null) return null;

            // skip 1 to ignore root (-1)
            string[] path = node.Path.Split(',').Skip(1).ToArray();
            // get all permissions matching either this node, its type, or an ancestor id
            List<UserGroupPermissionsPoco> allPermissions = _repo.AllPermissionsForNode(path, node.ContentType.Id);

            List<UserGroupPermissionsPoco> forNode = allPermissions.Where(p => p.NodeId == node.Id)?.ToList();
            if (forNode.Any())
            {
                return forNode;
            }

            List<UserGroupPermissionsPoco> forType =
                allPermissions.Where(p => p.ContentTypeId == node.ContentType.Id)?.ToList();

            if (forType.Any())
            {
                return forType;
            }

            // if we're here, reverse the path and check each node in turn
            IEnumerable<int> reversedPath = path.Reverse().Select(int.Parse);
            foreach (int ancestorId in reversedPath)
            {
                List<UserGroupPermissionsPoco> forAncestor = allPermissions.Where(x => x.NodeId == ancestorId)?.ToList();
                if (forAncestor.Any())
                {
                    return forAncestor;
                }
            }

            return new List<UserGroupPermissionsPoco>();
        }
    }
}
