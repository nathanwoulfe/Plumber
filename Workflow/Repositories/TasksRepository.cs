using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Repositories.Interfaces;
using Workflow.UnitOfWork;

namespace Workflow.Repositories
{
    public class TasksRepository : ITasksRepository
    {
        private readonly UmbracoDatabase _database;

        public TasksRepository()
            : this(ApplicationContext.Current.DatabaseContext.Database)
        {
        }

        public TasksRepository(UmbracoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="poco"></param>
        public void InsertTask(IUnitOfWork uow, WorkflowTaskInstancePoco poco)
        {
            uow.Db.Insert(poco);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="poco"></param>
        public void UpdateTask(IUnitOfWork uow, WorkflowTaskInstancePoco poco)
        {
            uow.Db.Update(poco);
        }

        /// <summary>
        /// Get a count of all pending tasks
        /// </summary>
        /// <returns>An integer representing the number of pending workflow tasks</returns>
        public int CountPendingTasks()
        {
            return _database.Fetch<int>(SqlHelpers.CountPendingTasks).First();
        }

        /// <summary>
        /// Get a count of all tasks assigned to the given group id
        /// </summary>
        /// <param name="groupId">The group id</param>
        /// <returns>An integer representing the number of pending workflow tasks assigned to the group</returns>
        public int CountGroupTasks(int groupId)
        {
            return _database.Fetch<int>(SqlHelpers.CountGroupTasks, groupId).First();
        }

        /// <summary>
        /// Get tasks and associated group by instance guid
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="guid">The instance guid</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetTasksAndGroupByInstanceId(IUnitOfWork uow, Guid guid)
        {
            return uow.Db.Fetch<WorkflowTaskInstancePoco>(SqlHelpers.TasksAndGroupByInstanceId, guid);
        }

        /// <summary>
        /// Get all tasks for the given node 
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetTasksByNodeId(int nodeId)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.TasksByNode, nodeId);
        }

        /// <summary>
        /// Get pending workflow tasks matching any of the provided status values
        /// </summary>
        /// <param name="status">A collection of WorkflowStatus integers</param>
        /// <param name="count">Number of items to return</param>
        /// <param name="page">Index of the page to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetPendingTasks(IEnumerable<int> status, int count, int page)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.PendingTasks, new { statusInts = status.Select(s => s.ToString()).ToArray() })
                .Skip((page - 1) * count).Take(count).ToList();
        }

        /// <summary>
        /// Get all pending workflow tasks matching any of the provided status values
        /// </summary>
        /// <param name="status">A collection of WorkflowStatus integers</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status)
        {

            return _database
                    .Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.PendingTasks,
                        new { statusInts = status.Select(s => s.ToString()).ToArray() }).ToList();

        }

        /// <summary>
        /// Get all tasks for the given group id
        /// </summary>
        /// <param name="groupId">Id of group to query</param>
        /// <param name="count">Number of items to return</param>
        /// <param name="page">Index of the page to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId, int count, int page)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.AllGroupTasks, groupId)
                .Skip((page - 1) * count).Take(count).ToList();
        }

        /// <summary>
        /// Get all tasks created after the given date
        /// </summary>
        /// <param name="oldest">The creation date of the oldest tasks to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest)
        {
            return _database.Fetch<WorkflowTaskInstancePoco>(SqlHelpers.AllTasksForDateRange, oldest);
        }

        /// <summary>
        /// Get all tasks created by the given user
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="id">The user id</param>
        /// <param name="status">The task status collection</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetTaskSubmissionsForUser(IUnitOfWork uow, int id, IEnumerable<int> status)
        {
            return uow.Db.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.SubmissionsForUser, new { id, statusInts = status.Select(s => s.ToString()).ToArray() });
        }
    }
}
