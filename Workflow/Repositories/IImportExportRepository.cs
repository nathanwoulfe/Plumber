using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Repositories
{
    public interface IImportExportRepository
    {
        WorkflowSettingsExport ExportSettings();
        IEnumerable<User2UserGroupExport> ExportUser2UserGroups();
        IEnumerable<UserGroupExport> ExportUserGroups();
        IEnumerable<UserGroupPermissionsExport> ExportUserGroupPermissions();
    }
}