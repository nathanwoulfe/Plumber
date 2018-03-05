using System.Data;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Workflow.UnitOfWork
{
    public class PetaPocoUnitOfWork : IUnitOfWork
    {
        private readonly Transaction _petaTransaction;
        private readonly UmbracoDatabase _db;

        public PetaPocoUnitOfWork()
        {
            _db = ApplicationContext.Current.DatabaseContext.Database;
            _petaTransaction = new Transaction(_db, IsolationLevel.Unspecified);
        }

        public void Dispose()
        {
            _petaTransaction.Dispose();
        }

        public void Commit()
        {
            _petaTransaction.Complete();
        }

        public UmbracoDatabase Db => _db;
    }
}
