namespace Workflow.UnitOfWork
{
    public class PetaPocoUnitOfWorkProvider : IUnitOfWorkProvider
    {
        public IUnitOfWork GetUnitOfWork()
        {
            return new PetaPocoUnitOfWork();
        }
    }
}
