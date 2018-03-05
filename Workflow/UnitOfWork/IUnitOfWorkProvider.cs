namespace Workflow.UnitOfWork
{
    public interface IUnitOfWorkProvider
    {
        IUnitOfWork GetUnitOfWork();
    }
}
