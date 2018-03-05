using System;
using System.Collections.Generic;
using Umbraco.Core;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services.Interfaces;
using Workflow.UnitOfWork;

namespace Workflow.Services
{
    public class TasksService : ITasksService
    {
        private readonly ITasksRepository _tasksRepo;
        private readonly IUnitOfWorkProvider _uow;

        public TasksService()
            : this(
                new TasksRepository(ApplicationContext.Current.DatabaseContext.Database),
                new PetaPocoUnitOfWorkProvider()
            )
        {
        }

        private TasksService(ITasksRepository tasksRepo, IUnitOfWorkProvider uow)
        {
            _tasksRepo = tasksRepo;
            _uow = uow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int CountPendingTasks()
        {
            return _tasksRepo.CountPendingTasks();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public int CountGroupTasks(int groupId)
        {
            return _tasksRepo.CountGroupTasks(groupId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<WorkflowTask> GetPendingTasks(IEnumerable<int> status, int count, int page)
        {
            List<WorkflowTaskInstancePoco> taskInstances = _tasksRepo.GetPendingTasks(status, count, page);
            List<WorkflowTask> tasks = taskInstances.ToWorkflowTaskList();

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<WorkflowTask> GetAllGroupTasks(int groupId, int count, int page)
        {
            List<WorkflowTaskInstancePoco> taskInstances = _tasksRepo.GetAllGroupTasks(groupId, count, page);
            List<WorkflowTask> tasks = taskInstances.ToWorkflowTaskList();

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status)
        {
            List<WorkflowTaskInstancePoco> taskInstances = _tasksRepo.GetAllPendingTasks(status);
            return taskInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest)
        {
            List<WorkflowTaskInstancePoco> taskInstances = _tasksRepo.GetAllTasksForDateRange(oldest);
            return taskInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetTasksByNodeId(int id)
        {
            List<WorkflowTaskInstancePoco> taskInstances = _tasksRepo.GetTasksByNodeId(id);
            return taskInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetTaskSubmissionsForUser(int id, IEnumerable<int> status)
        {
            using (IUnitOfWork uow = _uow.GetUnitOfWork())
            {
                return _tasksRepo.GetTaskSubmissionsForUser(uow, id, status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetTasksWithGroupByInstanceGuid(Guid guid)
        {
            using (IUnitOfWork uow = _uow.GetUnitOfWork())
            {
                return _tasksRepo.GetTasksAndGroupByInstanceId(uow, guid);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        public void InsertTask(WorkflowTaskInstancePoco poco)
        {
            using (IUnitOfWork uow = _uow.GetUnitOfWork())
            {
                _tasksRepo.InsertTask(uow, poco);
                uow.Commit();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        /// <returns></returns>
        public void UpdateTask(WorkflowTaskInstancePoco poco)
        {
            using (IUnitOfWork uow = _uow.GetUnitOfWork())
            {
                _tasksRepo.UpdateTask(uow, poco);
                uow.Commit();
            }
        }
    }
}
