using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Workflow.Models;
using Constants = Workflow.Helpers.Constants;

namespace Workflow.Startup
{
    public class CreateTables : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {            
            DatabaseContext dbContext = applicationContext.DatabaseContext;
            var helper = new DatabaseSchemaHelper(dbContext.Database, LoggerResolver.Current.Logger, dbContext.SqlSyntax);

            if (!helper.TableExist(Constants.SettingsTable))
            {
                helper.CreateTable<WorkflowSettingsPoco>(false);
            }

            if (!helper.TableExist(Constants.UserGroupsTable))
            {
                helper.CreateTable<UserGroupPoco>(false);
            }

            if (!helper.TableExist(Constants.User2UserGroupTable))
            {
                helper.CreateTable<User2UserGroupPoco>(false);
            }

            if (!helper.TableExist(Constants.PermissionsTable))
            {
                helper.CreateTable<UserGroupPermissionsPoco>(false);
            }

            if (!helper.TableExist(Constants.InstanceTable))
            {
                helper.CreateTable<WorkflowInstancePoco>(false);
            }

            if (!helper.TableExist(Constants.TaskInstanceTable))
            {
                helper.CreateTable<WorkflowTaskPoco>(false);
            }
        }
    }
}