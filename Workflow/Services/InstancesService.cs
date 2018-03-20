using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services.Interfaces;

namespace Workflow.Services
{
    public class InstancesService : IInstancesService
    {
        private readonly ILogger _log;
        private readonly IInstancesRepository _repo;
        private readonly ITasksService _tasksService;

        public InstancesService()
            : this(
                ApplicationContext.Current.ProfilingLogger.Logger,
                new InstancesRepository(), 
                new TasksService()
            )
        {
        }

        private InstancesService(ILogger log, IInstancesRepository repo, ITasksService tasksService)
        {
            _log = log;
            _repo = repo;
            _tasksService = tasksService;
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
            List<WorkflowInstance> workflowInstances = ConvertToWorkflowInstanceList(
                page.HasValue && count.HasValue ?
                instances.Skip((page.Value - 1) * count.Value).Take(count.Value).ToList() :
                instances);

            return workflowInstances;
        }

        /// <summary>
        /// Converts a list of instance pocos into UI-friendly instance models
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public List<WorkflowInstance> ConvertToWorkflowInstanceList(List<WorkflowInstancePoco> instances)
        {
            List<WorkflowInstance> workflowInstances = new List<WorkflowInstance>();

            if (instances == null || instances.Count <= 0)
                return workflowInstances.OrderByDescending(x => x.RequestedOn).ToList();

            foreach (WorkflowInstancePoco instance in instances)
            {
                var model = new WorkflowInstance
                {
                    Type = instance.TypeDescription,
                    Status = instance.StatusName,
                    CssStatus = instance.StatusName.ToLower().Split(' ')[0],
                    NodeId = instance.NodeId,
                    NodeName = instance.Node.Name,
                    RequestedBy = instance.AuthorUser.Name,
                    RequestedOn = instance.CreatedDate,
                    CompletedOn = instance.CompletedDate,
                    Tasks = _tasksService.ConvertToWorkflowTaskList(instance.TaskInstances.ToList(), instance)
                };

                workflowInstances.Add(model);
            }

            return workflowInstances.OrderByDescending(x => x.RequestedOn).ToList();
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
            _repo.InsertInstance(instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public void UpdateInstance(WorkflowInstancePoco instance)
        {
            _repo.UpdateInstance(instance);
        }
    }
}
