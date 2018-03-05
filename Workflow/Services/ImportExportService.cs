﻿using System;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services.Interfaces;

namespace Workflow.Services
{
    public class ImportExportService : IImportExportService
    {
        private readonly ILogger _log;
        private readonly IImportExportRepository _repo;

        public ImportExportService()
            : this(
                  ApplicationContext.Current.ProfilingLogger.Logger,
                  new ImportExportRepository(ApplicationContext.Current.DatabaseContext.Database)
            )
        {
        }

        public ImportExportService(ILogger log, IImportExportRepository repo)
        {
            _log = log;
            _repo = repo;
        }

        /// <summary>
        /// Export all workflow configuration. This is a simple representation of the current workflow database tables.
        /// </summary>
        /// <returns></returns>
        public Task<ImportExportModel> Export()
        {
            var model = new ImportExportModel();

            try
            {
                model.Settings = _repo.ExportSettings();
                model.UserGroups = _repo.ExportUserGroups();
                model.User2UserGroup = _repo.ExportUser2UserGroups();
                model.UserGroupPermissions = _repo.ExportUserGroupPermissions();
            }
            catch (Exception e)
            {
                _log.Error(GetType(), e.Message, e);
            }

            return Task.FromResult(model);
        }

        /// <summary>
        /// Import all workflow configuration
        /// </summary>
        /// <param name="model">A model representing the end-to-end workflow configuration</param>
        /// <returns></returns>
        public Task<bool> Import(ImportExportModel model)
        {
            if (model.Settings != null)
            {
                _repo.ImportSettings(model.Settings);
            }

            if (model.UserGroups != null)
            {
                _repo.ImportUserGroups(model.UserGroups);
            }

            if (model.User2UserGroup != null)
            {
                _repo.ImportUser2UserGroups(model.User2UserGroup);
            }

            if (model.UserGroupPermissions != null)
            {
                _repo.ImportUserGroupPermissions(model.UserGroupPermissions);
            }

            return Task.FromResult(true);
        }
    }
}
