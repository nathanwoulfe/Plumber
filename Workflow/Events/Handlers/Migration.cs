using System;
using System.Collections.Generic;
using System.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web;
using Workflow.Helpers;

namespace Workflow.Events.Handlers
{
    public class Migration : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext context)
        {
            DoMigration();
        }

        private static void DoMigration()
        {
            var currentVersion = new SemVersion(0);

            IEnumerable<IMigrationEntry> migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(MagicStrings.Name);
            IMigrationEntry latest = migrations.OrderByDescending(x => x.Version).FirstOrDefault();

            if (null != latest)
            {
                currentVersion = latest.Version;
            }

            var targetVersion = new SemVersion(0, 9);
            if (targetVersion == currentVersion)
            {
                return;
            }

            var migrationsRunner = new MigrationRunner(
                ApplicationContext.Current.Services.MigrationEntryService,
                ApplicationContext.Current.ProfilingLogger.Logger,
                currentVersion,
                targetVersion,
                MagicStrings.Name);

            try
            {
                migrationsRunner.Execute(UmbracoContext.Current.Application.DatabaseContext.Database);
            }
            catch (Exception ex)
            {
                LogHelper.Error<Migration>("Error running Plumber migration", ex);
            }
        }
    }
}
