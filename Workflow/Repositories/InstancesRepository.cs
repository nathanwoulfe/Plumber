using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Extensions;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Relators;
using Workflow.Repositories.Interfaces;

namespace Workflow.Repositories
{
    public class InstancesRepository : IInstancesRepository
    {
        private readonly UmbracoDatabase _database;

        public InstancesRepository()
            : this(ApplicationContext.Current.DatabaseContext.Database)
        {
        }

        private InstancesRepository(UmbracoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        public void InsertInstance(WorkflowInstancePoco poco)
        {
            _database.Insert(poco);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        public void UpdateInstance(WorkflowInstancePoco poco)
        {
            _database.Update(poco);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int CountPendingInstances()
        {
            return _database.Fetch<int>(SqlQueries.CountPendingInstances).First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double CountAllInstances()
        {
            return _database.Fetch<double>(SqlQueries.CountAllInstances).First();
        }

        /// <summary>
        /// Get all workflow instances
        /// </summary>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public List<WorkflowInstancePoco> GetAllInstances()
        {
            return _database.Fetch<WorkflowInstancePoco, WorkflowTaskPoco, UserGroupPoco, WorkflowInstancePoco>
                (new UserToGroupForInstanceRelator().MapIt, SqlQueries.AllInstances);
        }

        /// <summary>
        /// Get all active workflow instances
        /// </summary>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public List<WorkflowInstancePoco> GetAllActiveInstances()
        {
            return _database.Fetch<WorkflowInstancePoco>(SqlQueries.AllActiveInstances);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public List<WorkflowInstancePoco> GetAllInstancesForNode(int nodeId)
        {
            return _database.Fetch<WorkflowInstancePoco, WorkflowTaskPoco, UserGroupPoco, WorkflowInstancePoco>
                (new UserToGroupForInstanceRelator().MapIt, SqlQueries.AllInstancesForNode, nodeId);
        }

        /// <summary>
        /// Get all workflow instances created after the given date
        /// </summary>
        /// <param name="oldest">The creation date of the oldest instances to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public List<WorkflowInstancePoco> GetAllInstancesForDateRange(DateTime oldest)
        {
            return _database.Fetch<WorkflowInstancePoco>(SqlQueries.AllInstancesForDateRange, oldest);
        }

        /// <summary>
        /// Get all workflow instances created after the given date
        /// </summary>
        /// <param name="oldest">The creation date of the oldest instances to return</param>
        /// <param name="filter"></param>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public List<WorkflowInstancePoco> GetFilteredPagedInstancesForDateRange(DateTime oldest, string filter)
        {
            int filterVal = !string.IsNullOrEmpty(filter) ? (int)Enum.Parse(typeof(WorkflowStatus), filter) : -1;
            return _database.Fetch<WorkflowInstancePoco, WorkflowTaskPoco, UserGroupPoco, WorkflowInstancePoco>
                (new UserToGroupForInstanceRelator().MapIt, SqlQueries.FilteredInstancesForDateRange, oldest, filterVal);
        }

        /// <summary>
        /// Get a single instance by guid
        /// </summary>
        /// <param name="guid">The instance guid</param>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public WorkflowInstancePoco GetInstanceByGuid(Guid guid)
        {
            return _database.Fetch<WorkflowInstancePoco>(SqlQueries.InstanceByGuid, guid).First();
        }

        /// <summary>
        /// Get all instances matching the given status[es] for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <param name="status">Optional list of WorkflowStatus integers. If not provided, method returns all instances for the node.</param>
        /// <returns>A list of objects of type <see cref="WorkflowInstancePoco"/></returns>
        public IEnumerable<WorkflowInstancePoco> GetInstancesForNodeByStatus(int nodeId, IEnumerable<int> status = null)
        {
            if (status == null || !status.Any())
                return _database.Fetch<WorkflowInstancePoco>(SqlQueries.InstanceByNodeStr, nodeId);

            string statusStr = string.Concat("Status = ", string.Join(" OR Status = ", status));
            if (statusStr.HasValue())
            {
                statusStr = $" AND ({statusStr})";
            }

            return _database.Fetch<WorkflowInstancePoco>(string.Concat(SqlQueries.InstanceByNodeStr, statusStr), nodeId);
        }
    }
}
