using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Services.Interfaces
{
    public interface IInstancesService
    {
        IEnumerable<WorkflowInstancePoco> GetAll();
        IEnumerable<WorkflowInstancePoco> GetForNodeByStatus(int nodeId, IEnumerable<int> status);

        List<WorkflowInstanceViewModel> Get(int? page, int? count);
        List<WorkflowInstanceViewModel> GetByNodeId(int nodeId, int? page, int? count);
        List<WorkflowInstanceViewModel> GetAllInstancesForDateRange(DateTime? oldest);
        List<WorkflowInstanceViewModel> ConvertToWorkflowInstanceList(List<WorkflowInstancePoco> instance);
        List<WorkflowInstanceViewModel> GetFilteredPagedInstancesForDateRange(DateTime oldest, int? count, int? page, string filter = "");

        Dictionary<int, bool> IsActive(IEnumerable<int> ids);

        WorkflowInstancePoco GetByGuid(Guid guid);
        WorkflowInstancePoco GetPopulatedInstance(Guid guid);

        int CountPending();
        double CountAll(); // value is used to calculate total pages, using math.ceiling, so needs type

        void InsertInstance(WorkflowInstancePoco instance);
        void UpdateInstance(WorkflowInstancePoco instance);
    }
}
