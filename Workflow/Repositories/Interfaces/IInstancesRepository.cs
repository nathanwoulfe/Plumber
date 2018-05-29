using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Repositories.Interfaces
{
    public interface IInstancesRepository
    {
        /// <summary>
        /// Persist a new workflow instance to the database
        /// </summary>
        /// <param name="poco"></param>
        void InsertInstance(WorkflowInstancePoco poco);

        /// <summary>
        /// Persists an updated workflow instance to the database
        /// </summary>
        /// <param name="poco"></param>
        void UpdateInstance(WorkflowInstancePoco poco);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int CountPendingInstances();

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <returns></returns>
        double CountAllInstances();

        /// <summary>
        /// Get all workflow instances, regardless of status
        /// </summary>       
        /// <returns></returns>
        List<WorkflowInstancePoco> GetAllInstances();

        /// <summary>
        /// Get all workflow instances, regardless of status, for the given node id
        /// </summary>       
        /// <param name="nodeId"></param>
        /// <returns></returns>
        List<WorkflowInstancePoco> GetAllInstancesForNode(int nodeId);

        /// <summary>
        /// Get all instances created after the provided date
        /// </summary>
        /// <param name="oldest"></param>
        /// <returns></returns>
        List<WorkflowInstancePoco> GetAllInstancesForDateRange(DateTime oldest);

        /// <summary>
        /// Get all instances for the given node, provided they match one of the status values
        /// </summary>
        /// <param name="node"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        IEnumerable<WorkflowInstancePoco> GetInstancesForNodeByStatus(int node, IEnumerable<int> status = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        List<WorkflowInstancePoco> GetFilteredPagedInstancesForDateRange(DateTime oldest, string filter);

        /// <summary>
        /// Gets the instances corresponding to the guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        WorkflowInstancePoco GetInstanceByGuid(Guid guid);
    }
}
