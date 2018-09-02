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
        public List<WorkflowInstance> Get(int? page = 0, int? count = null)
        {
            List<WorkflowInstancePoco> instances = _repo.GetAllInstances();
            
            // todo - fetch only required data, don't do paging here
            instances = page.HasValue && count.HasValue
                ? instances.Skip((page.Value - 1) * count.Value).Take(count.Value).ToList()
                : instances;

            List<WorkflowInstance> workflowInstances = ConvertToWorkflowInstanceList(instances);

            return workflowInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <returns></returns>
        public List<WorkflowInstance> GetAllInstancesForDateRange(DateTime? oldest)
        {
            List<WorkflowInstancePoco> instances = _repo.GetAllInstancesForDateRange(oldest.Value);
            List<WorkflowInstance> workflowInstances = ConvertToWorkflowInstanceList(instances);

            return workflowInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="page"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<WorkflowInstance> GetByNodeId(int nodeId, int? page, int? count)
        {
            List<WorkflowInstancePoco> instances = _repo.GetAllInstancesForNode(nodeId);

            // todo - fetch only required data, don't do paging here
            instances = page.HasValue && count.HasValue
                ? instances.Skip((page.Value - 1) * count.Value).Take(count.Value).ToList()
                : instances;

            List<WorkflowInstance> workflowInstances = ConvertToWorkflowInstanceList(instances);

            return workflowInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<WorkflowInstance> GetFilteredPagedInstancesForDateRange(DateTime oldest, int? count, int? page, string filter = "")
        {
            List<WorkflowInstancePoco> instances = _repo.GetFilteredPagedInstancesForDateRange(oldest, filter);

            // todo - fetch only required data, don't do paging here
            instances = page.HasValue && count.HasValue
                ? instances.Skip((page.Value - 1) * count.Value).Take(count.Value).ToList()
                : instances;

            List<WorkflowInstance> workflowInstances = ConvertToWorkflowInstanceList(instances);

            return workflowInstances;
        }

        /// <summary>
        /// Check the status of a set of nodes - do they have a current workflow process?
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public Dictionary<int, bool> IsActive(IEnumerable<int> ids)
        {
            List<WorkflowInstancePoco> allInstances = _repo.GetAllActiveInstances();

            Dictionary<int, bool> response = ids.ToDictionary(
                k => k, 
                v => allInstances.Any(i => i.NodeId == v));

            return response;
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
                return workflowInstances;

            foreach (WorkflowInstancePoco instance in instances)
            {
                var model = new WorkflowInstance
                {
                    Type = instance.TypeDescription,
                    InstanceGuid = instance.Guid,
                    Status = instance.StatusName,
                    CssStatus = instance.StatusName.ToLower().Split(' ')[0],
                    NodeId = instance.NodeId,
                    NodeName = instance.Node?.Name,
                    RequestedBy = instance.AuthorUser?.Name,
                    RequestedOn = instance.CreatedDate.ToFriendlyDate(),
                    CreatedDate = instance.CreatedDate,
                    CompletedDate = instance.CompletedDate,
                    Comment = instance.AuthorComment,
                    Tasks = _tasksService.ConvertToWorkflowTaskList(instance.TaskInstances.ToList(), false, instance)
                };

                workflowInstances.Add(model);
            }

            return workflowInstances.OrderByDescending(x => x.CreatedDate).ToList();
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
        /// <returns></returns>
        public double CountAll()
        {
            return _repo.CountAllInstances();
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
