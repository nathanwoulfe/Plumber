using Semver;
using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web;
using Workflow.Helpers;

namespace Workflow.Startup
{
    public class MigrationEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext context)
        {
            DoMigration();
        }

        private static void DoMigration()
        {
            var currentVersion = new SemVersion(0);

            var migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(MagicStrings.Name);
            var latest = migrations.OrderByDescending(x => x.Version).FirstOrDefault();

            if (null != latest)
            {
                currentVersion = latest.Version;
            }

            var targetVersion = new SemVersion(0, 3);
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
                LogHelper.Error<MigrationEvents>("Error running Plumber migration", ex);
            }
        }
    }
}
