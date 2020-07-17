using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Models;
using Workflow.Repositories.Interfaces;

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

        private ImportExportRepository(UmbracoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Export the settings as an importable model
        /// </summary>
        /// <returns>Object of type <see cref="WorkflowSettingsExport"/></returns>
        public WorkflowSettingsExport ExportSettings()
        {
            var poco = _database.FirstOrDefault<WorkflowSettingsPoco>("SELECT * FROM WorkflowSettings");
            if (poco != null)
            {
                return new WorkflowSettingsExport
                {
                    DefaultApprover = poco.DefaultApprover,
                    EditUrl = poco.EditUrl,
                    Email = poco.Email,
                    ExcludeNodes = poco.ExcludeNodes,
                    FlowType = poco.FlowType,
                    SendNotifications = poco.SendNotifications,
                    SiteUrl = poco.SiteUrl
                };
            }

            return new WorkflowSettingsExport();
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

        /// <summary>
        /// Import the settings export model back into the database. This is destructive.
        /// </summary>
        /// <param name="model">Object of type <see cref="WorkflowSettingsExport"/></param>
        public void ImportSettings(WorkflowSettingsExport model)
        {
            // delete first as this is an import and should be destructive
            _database.Execute("DELETE FROM WorkflowSettings");
            _database.Insert(new WorkflowSettingsPoco
            {
                DefaultApprover = model.DefaultApprover,
                EditUrl = model.EditUrl,
                Email = model.Email,
                ExcludeNodes = model.ExcludeNodes,
                FlowType = model.FlowType,
                SendNotifications = model.SendNotifications,
                SiteUrl = model.SiteUrl
            });
        }

        /// <summary>
        /// Import the user group export model back into the database. This is destructive.
        /// </summary>
        /// <param name="model">Ienumerable of objects of type <see cref="UserGroupExport"/></param>
        public void ImportUserGroups(IEnumerable<UserGroupExport> model)
        {
            // delete first as this is an import and should be destructive
            _database.Execute("DELETE FROM WorkflowUserGroups");
            foreach (UserGroupExport m in model)
            {
                _database.Insert(new UserGroupPoco
                {
                    Alias = m.Alias,
                    Deleted = m.Deleted,
                    Description = m.Description,
                    GroupEmail = m.GroupEmail,
                    AdditionalGroupEmails = m.AdditionalEmails,
                    GroupId = m.GroupId,
                    Name = m.Name
                });
            }
        }

        /// <summary>
        /// Import the user 2 user group export model back into the database. This is destructive.
        /// </summary>
        /// <param name="model">Ienumerable of objects of type <see cref="User2UserGroupExport"/></param>
        public void ImportUser2UserGroups(IEnumerable<User2UserGroupExport> model)
        {
            // delete first as this is an import and should be destructive
            _database.Execute("DELETE FROM WorkflowUser2UserGroup");
            foreach (User2UserGroupExport m in model)
            {
                _database.Insert(new User2UserGroupPoco
                {
                    GroupId = m.GroupId,
                    UserId = m.UserId
                });
            }
        }

        /// <summary>
        /// Import the usergroup permission export model back into the database. This is destructive.
        /// </summary>
        /// <param name="model">Ienumerable of objects of type <see cref="UserGroupPermissionsExport"/></param>
        public void ImportUserGroupPermissions(IEnumerable<UserGroupPermissionsExport> model)
        {
            // delete first as this is an import and should be destructive
            _database.Execute("DELETE FROM WorkflowUserGroupPermissions");
            foreach (UserGroupPermissionsExport m in model)
            {
                _database.Insert(new UserGroupPermissionsPoco
                {
                    ContentTypeId = m.ContentTypeId,
                    GroupId = m.GroupId,
                    NodeId = m.NodeId,
                    Permission = m.Permission,
                    Condition = m.Condition
                });
            }
        }
    }
}
