using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Workflow.Helpers;
using Workflow.Models;
using Workflow.Repositories.Interfaces;

namespace Workflow.Repositories
{
    public class TasksRepository : ITasksRepository
    {
        private readonly UmbracoDatabase _database;

        public TasksRepository()
            : this(ApplicationContext.Current.DatabaseContext.Database)
        {
        }

        private TasksRepository(UmbracoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        public void InsertTask(WorkflowTaskPoco poco)
        {
            _database.Insert(poco);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        public void UpdateTask(WorkflowTaskPoco poco)
        {
            _database.Update(poco);
        }

        /// <summary>
        /// Get a count of all pending tasks
        /// </summary>
        /// <returns>An integer representing the number of pending workflow tasks</returns>
        public int CountPendingTasks()
        {
            return _database.Fetch<int>(SqlQueries.CountPendingTasks).First();
        }

        /// <summary>
        /// Get a count of all tasks assigned to the given group id
        /// </summary>
        /// <param name="groupId">The group id</param>
        /// <returns>An integer representing the number of pending workflow tasks assigned to the group</returns>
        public int CountGroupTasks(int groupId)
        {
            return _database.Fetch<int>(SqlQueries.CountGroupTasks, groupId).First();
        }

        /// <summary>
        /// Get tasks and associated group by instance guid
        /// </summary>
        /// <param name="guid">The instance guid</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskPoco"/></returns>
        public List<WorkflowTaskPoco> GetTasksAndGroupByInstanceId(Guid guid)
        {
            return _database.Fetch<WorkflowTaskPoco, UserGroupPoco>(SqlQueries.TasksAndGroupByInstanceId, guid);
        }

        /// <summary>
        /// Get all tasks for the given node 
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskPoco"/></returns>
        public List<WorkflowTaskPoco> GetTasksByNodeId(int nodeId)
        {
            return _database.Fetch<WorkflowTaskPoco, WorkflowInstancePoco, UserGroupPoco>(SqlQueries.TasksByNode, nodeId);
        }

        /// <summary>
        /// Get all pending workflow tasks matching any of the provided status values
        /// </summary>
        /// <param name="status">A collection of WorkflowStatus integers</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskPoco"/></returns>
        public IEnumerable<WorkflowTaskPoco> GetAllPendingTasks(IEnumerable<int> status)
        {

            return _database
                    .Fetch<WorkflowTaskPoco, WorkflowInstancePoco, UserGroupPoco>(SqlQueries.PendingTasks,
                        new { statusInts = status.Select(s => s.ToString()).ToArray() }).ToList();

        }

        /// <summary>
        /// Get a single task by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WorkflowTaskPoco Get(int id)
        {
            return _database
                .Fetch<WorkflowTaskPoco, WorkflowInstancePoco>(SqlQueries.GetTask, id).FirstOrDefault();
        }

        /// <summary>
        /// Get all tasks for the given group id
        /// </summary>
        /// <param name="groupId">Id of group to query</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskPoco"/></returns>
        public IEnumerable<WorkflowTaskPoco> GetAllGroupTasks(int groupId)
        {
            return _database
                .Fetch<WorkflowTaskPoco, WorkflowInstancePoco, UserGroupPoco>(SqlQueries.AllGroupTasks, groupId).ToList();
        }

        /// <summary>
        /// Get all tasks created after the given date
        /// </summary>
        /// <param name="oldest">The creation date of the oldest tasks to return</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskPoco"/></returns>
        public List<WorkflowTaskPoco> GetAllTasksForDateRange(DateTime oldest)
        {
            return _database.Fetch<WorkflowTaskPoco>(SqlQueries.AllTasksForDateRange, oldest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<WorkflowTaskPoco> GetFilteredPagedTasksForDateRange(DateTime oldest, string filter)
        {
            int filterVal = !string.IsNullOrEmpty(filter) ? (int)Enum.Parse(typeof(TaskStatus), filter) : -1;
            return _database.Fetch<WorkflowTaskPoco, WorkflowInstancePoco, UserGroupPoco>(SqlQueries.FilteredTasksForDateRange, oldest, filterVal);
        }

        /// <summary>
        /// Get all tasks created by the given user
        /// </summary>
        /// <param name="id">The user id</param>
        /// <param name="status">The task status collection</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskPoco"/></returns>
        public List<WorkflowTaskPoco> GetTaskSubmissionsForUser(int id, IEnumerable<int> status)
        {
            return _database.Fetch<WorkflowTaskPoco, WorkflowInstancePoco, UserGroupPoco>(SqlQueries.SubmissionsForUser, new { id, statusInts = status.Select(s => s.ToString()).ToArray() });
        }
    }
}
