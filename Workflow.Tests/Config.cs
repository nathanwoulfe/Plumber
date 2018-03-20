using System.Configuration;
using System.Data.Common;
using NUnit.Framework;

namespace Workflow.Tests
{
    [SetUpFixture]
    public class Config
    {
        public DbConnection _conn;

        [OneTimeSetUp]
        public void Init()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString;

            _conn = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0").;
            _conn.ConnectionString = connectionString;
            _conn.Open();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
        }

        public DbConnection Database()
        {
            return _conn;
        }
    }
}
