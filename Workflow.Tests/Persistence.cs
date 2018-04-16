using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Workflow.Tests
{
    public static class Persistence
    {
        public static DatabaseSchemaHelper Helper()
        {
            DatabaseContext dbContext = ApplicationContext.Current.DatabaseContext;
            return new DatabaseSchemaHelper(dbContext.Database, LoggerResolver.Current.Logger, dbContext.SqlSyntax);
        }
    }
}
