using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Repositories.Interfaces
{
    public interface IImportExportRepository
    {
        WorkflowSettingsExport ExportSettings();
        IEnumerable<User2UserGroupExport> ExportUser2UserGroups();
        IEnumerable<UserGroupExport> ExportUserGroups();
        IEnumerable<UserGroupPermissionsExport> ExportUserGroupPermissions();

        void ImportSettings(WorkflowSettingsExport model);
        void ImportUserGroups(IEnumerable<UserGroupExport> model);
        void ImportUser2UserGroups(IEnumerable<User2UserGroupExport> model);
        void ImportUserGroupPermissions(IEnumerable<UserGroupPermissionsExport> model);
    }
}