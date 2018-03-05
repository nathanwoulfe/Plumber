using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services.Interfaces;
using Workflow.UnitOfWork;

namespace Workflow.Services
{
    public class InstancesService : IInstancesService
    {
        private readonly ILogger _log;
        private readonly IInstancesRepository _repo;
        private readonly IUnitOfWorkProvider _uow;

        public InstancesService()
            : this(
                ApplicationContext.Current.ProfilingLogger.Logger,
                new InstancesRepository(ApplicationContext.Current.DatabaseContext.Database), 
                new PetaPocoUnitOfWorkProvider()
            )
        {
        }

        private InstancesService(ILogger log, IInstancesRepository repo, IUnitOfWorkProvider uow)
        {
            _log = log;
            _repo = repo;
            _uow = uow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public IEnumerable<WorkflowInstancePoco> GetForNodeByStatus(int nodeId, IEnumerable<int> status)
        {
            IEnumerable<WorkflowInstancePoco> instances = _repo.GetInstancesForNodeByStatus(nodeId, status);

            return instances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WorkflowInstancePoco> GetAll()
        {
            return _repo.GetAllInstances();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<WorkflowInstance> Get(int? page = 0, int? count = null, DateTime? oldest = null)
        {
            List<WorkflowInstancePoco> instances = oldest.HasValue ? _repo.GetAllInstancesForDateRange(oldest.Value) : _repo.GetAllInstances();

            // todo - fetch only required data, don't do paging here
            List<WorkflowInstance> workflowInstances = (page.HasValue && count.HasValue ?
                instances.Skip((page.Value - 1) * count.Value).Take(count.Value).ToList() :
                instances).ToWorkflowInstanceList();

            return workflowInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public WorkflowInstancePoco GetByGuid(Guid guid)
        {
            WorkflowInstancePoco instance = _repo.GetInstanceByGuid(guid);
            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int CountPending()
        {
            return _repo.CountPendingInstances();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public void InsertInstance(WorkflowInstancePoco instance)
        {
            using (IUnitOfWork uow = _uow.GetUnitOfWork())
            {
                _repo.InsertInstance(uow, instance);
                uow.Commit();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public void UpdateInstance(WorkflowInstancePoco instance)
        {
            using (IUnitOfWork uow = _uow.GetUnitOfWork())
            {
                _repo.UpdateInstance(uow, instance);
                uow.Commit();
            }
        }
    }
}
