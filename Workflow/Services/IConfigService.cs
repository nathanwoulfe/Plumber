using System.Collections.Generic;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services
{
    public interface IConfigService
    {
        Task<bool> UpdateNodeConfigAsync(Dictionary<int, List<UserGroupPermissionsPoco>> model);
        Task<bool> UpdateContentTypeConfigAsync(Dictionary<int, List<UserGroupPermissionsPoco>> model);
    }
}