using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Workflow.Models;

namespace Workflow.Actions
{
    public class RegisterEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {            
            var dbContext = applicationContext.DatabaseContext;
            var helper = new DatabaseSchemaHelper(dbContext.Database, LoggerResolver.Current.Logger, dbContext.SqlSyntax);

            if (!helper.TableExist("WorkflowSettings"))
            {
                helper.CreateTable<WorkflowSettingsPoco>(false);
            }

            if (!helper.TableExist("WorkflowUserGroups"))
            {
                helper.CreateTable<UserGroupPoco>(false);
            }

            if (!helper.TableExist("WorkflowUser2UserGroup"))
            {
                helper.CreateTable<User2UserGroupPoco>(false);
            }

            if (!helper.TableExist("WorkflowUserGroupPermissions"))
            {
                helper.CreateTable<UserGroupPermissionsPoco>(false);
            }

            if (!helper.TableExist("WorkflowInstance"))
            {
                helper.CreateTable<WorkflowInstancePoco>(false);
            }

            if (!helper.TableExist("WorkflowTaskInstance"))
            {
                helper.CreateTable<WorkflowTaskInstancePoco>(false);
            }
        }
    }
}