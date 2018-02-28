using Chauffeur;
using Workflow.Repositories;
using Workflow.Services;

namespace Workflow.Chauffeur
{
    public class WorkflowDependencies : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register<GroupService>().As<IGroupService>();
            container.Register<PocoRepository>().As<IPocoRepository>();
        }
    }
}
