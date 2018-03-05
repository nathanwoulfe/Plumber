using System.Collections.Generic;
using Umbraco.Core.Models;
using Workflow.Models;

namespace Workflow.Services.Interfaces
{
    public interface IConfigService
    {
        bool UpdateNodeConfig(Dictionary<int, List<UserGroupPermissionsPoco>> model);
        bool UpdateContentTypeConfig(Dictionary<int, List<UserGroupPermissionsPoco>> model);
        bool HasFlow(int nodeId);

        List<UserGroupPermissionsPoco> GetPermissionsForNode(int nodeId, int? contentTypeId);
        List<UserGroupPermissionsPoco> GetRecursivePermissionsForNode(IPublishedContent node);
    }
}