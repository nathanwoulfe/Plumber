using System.Data.Common;
using System.Data.Entity;
using Workflow.Models;

namespace Workflow.Tests
{
    public class Entities : DbContext
    {
        public Entities()
        {
        }

        public Entities(DbConnection connection)
            : base(connection, true)
        {
        }

        public class TestGroupPoco : UserGroupPoco
        {
        }
    }
}
