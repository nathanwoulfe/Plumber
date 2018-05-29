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
        public void InsertTask(WorkflowTaskInstancePoco poco)
        {
            _database.Insert(poco);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        public void UpdateTask(WorkflowTaskInstancePoco poco)
        {
            _database.Update(poco);
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
        /// <param name="guid">The instance guid</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetTasksAndGroupByInstanceId(Guid guid)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, UserGroupPoco>(SqlHelpers.TasksAndGroupByInstanceId, guid);
        }

        /// <summary>
        /// Get pending workflow tasks matching any of the provided status values
        /// </summary>
        /// <param name="status">A collection of WorkflowStatus integers</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public IEnumerable<WorkflowTaskInstancePoco> GetPendingTasks(IEnumerable<int> status)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(
                SqlHelpers.PendingTasks, new {statusInts = status.Select(s => s.ToString()).ToArray()});
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
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public IEnumerable<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId)
        {
            return _database
                .Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.AllGroupTasks, groupId).ToList();
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
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<WorkflowTaskInstancePoco> GetFilteredPagedTasksForDateRange(DateTime oldest, string filter)
        {
            int filterVal = !string.IsNullOrEmpty(filter) ? (int)Enum.Parse(typeof(TaskStatus), filter) : -1;
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.FilteredTasksForDateRange, oldest, filterVal);
        }

        /// <summary>
        /// Get all tasks created by the given user
        /// </summary>
        /// <param name="id">The user id</param>
        /// <param name="status">The task status collection</param>
        /// <returns>A list of objects of type <see cref="WorkflowTaskInstancePoco"/></returns>
        public List<WorkflowTaskInstancePoco> GetTaskSubmissionsForUser(int id, IEnumerable<int> status)
        {
            return _database.Fetch<WorkflowTaskInstancePoco, WorkflowInstancePoco, UserGroupPoco>(SqlHelpers.SubmissionsForUser, new { id, statusInts = status.Select(s => s.ToString()).ToArray() });
        }
    }
}
