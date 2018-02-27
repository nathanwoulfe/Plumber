using System;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Workflow.Models;
using Workflow.Repositories;

namespace Workflow.Services
{
    public class ImportExportService : IImportExportService
    {
        private readonly ILogger log;
        private readonly IImportExportRepository repo;

        public ImportExportService()
            : this(
                  ApplicationContext.Current.ProfilingLogger.Logger,
                  new ImportExportRepository(ApplicationContext.Current.DatabaseContext.Database)
            )
        {
        }

        public ImportExportService(ILogger log, IImportExportRepository repo)
        {
            this.log = log;
            this.repo = repo;
        }

        /// <summary>
        /// Export all workflow configuration. This is a simple representation of the current workflow database tables.
        /// </summary>
        /// <returns></returns>
        public Task<ImportExportModel> Export()
        {
            var model = new ImportExportModel
            {
                Settings = repo.ExportSettings(),
                UserGroups = repo.ExportUserGroups(),
                User2UserGroup = repo.ExportUser2UserGroups(),
                UserGroupPermissions = repo.ExportUserGroupPermissions()
            };

            return Task.FromResult(model);
        }

        /// <summary>
        /// Import all workflow configuration
        /// </summary>
        /// <param name="model">A model representing the end-to-end workflow configuration</param>
        /// <returns></returns>
        public Task Import(ImportExportModel model)
        {
            throw new NotImplementedException();
        }
    }
}
