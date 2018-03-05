using System;
using Umbraco.Core.Persistence;

namespace Workflow.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();

        UmbracoDatabase Db { get; }
    }
}
