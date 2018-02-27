using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Models;

namespace Workflow.Repositories
{
    /// <summary>
    /// The class responsible for all interactions with the workflow tables in the Umbraco database
    /// </summary>
    public class ImportExportRepository : IImportExportRepository
    {
        private readonly UmbracoDatabase _database;

        public ImportExportRepository()
            : this(ApplicationContext.Current.DatabaseContext.Database)
        {
        }

        public ImportExportRepository(UmbracoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Export the settings as an importable model
        /// </summary>
        /// <returns>Object of type <see cref="WorkflowSettingsExport"/></returns>
        public WorkflowSettingsExport ExportSettings()
        {
            return _database.Fetch<WorkflowSettingsExport>("SELECT * FROM WorkflowSettings").First();
        }

        /// <summary>
        /// Export the user2usergroup objects as an importable model
        /// </summary>
        /// <returns>Object of type <see cref="User2UserGroupExport"/></returns>
        public IEnumerable<User2UserGroupExport> ExportUser2UserGroups()
        {
            return _database.Fetch<User2UserGroupExport>("SELECT * FROM WorkflowUser2UserGroup");
        }

        /// <summary>
        /// Export the settings as an importable model
        /// </summary>
        /// <returns>Object of type <see cref="WorkflowSettingsExport"/></returns>
        public IEnumerable<UserGroupExport> ExportUserGroups()
        {
            return _database.Fetch<UserGroupExport>("SELECT * FROM WorkflowUserGroups");
        }

        /// <summary>
        /// Export the user group permissions as an importable model
        /// </summary>
        /// <returns>Object of type <see cref="UserGroupPermissionsExport"/></returns>
        public IEnumerable<UserGroupPermissionsExport> ExportUserGroupPermissions()
        {
            return _database.Fetch<UserGroupPermissionsExport>("SELECT * FROM WorkflowUserGroupPermissions");
        }
    }
}
