using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Workflow.Models;

namespace Workflow.Tests.Persistence
{
    [TestFixture]
    public class CreateDatabaseTables : Config
    {
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void Init(int testCase)
        {
            int functionResult = 1 + testCase;

            Assert.IsTrue(functionResult == 3);
        }

        public void CanCreateTables()
        {
            var helper = new DatabaseSchemaHelper(, LoggerResolver.Current.Logger, dbContext.SqlSyntax);

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
