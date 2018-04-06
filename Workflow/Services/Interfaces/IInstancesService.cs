using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Services.Interfaces
{
    public interface IInstancesService
    {
        IEnumerable<WorkflowInstancePoco> GetAll();
        IEnumerable<WorkflowInstancePoco> GetForNodeByStatus(int nodeId, IEnumerable<int> status);

        List<WorkflowInstance> Get(int? page, int? count, DateTime? oldest);
        WorkflowInstancePoco GetByGuid(Guid guid);

        int CountPending();
        double CountAll(); // value is used to calculate total pages, using math.ceiling, so needs type

        void InsertInstance(WorkflowInstancePoco instance);
        void UpdateInstance(WorkflowInstancePoco instance);
    }
}
