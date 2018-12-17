using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Repositories.Interfaces
{
    public interface ITasksRepository
    {
        void InsertTask(WorkflowTaskPoco poco);
        void UpdateTask(WorkflowTaskPoco poco);

        int CountGroupTasks(int groupId);
        int CountPendingTasks();

        IEnumerable<WorkflowTaskPoco> GetAllGroupTasks(int groupId);
        IEnumerable<WorkflowTaskPoco> GetAllPendingTasks(IEnumerable<int> status);

        WorkflowTaskPoco Get(int id);

        List<WorkflowTaskPoco> GetAllTasksForDateRange(DateTime oldest);
        List<WorkflowTaskPoco> GetFilteredPagedTasksForDateRange(DateTime oldest, string filter);
        List<WorkflowTaskPoco> GetTasksByNodeId(int nodeId);
        List<WorkflowTaskPoco> GetTaskSubmissionsForUser(int id, IEnumerable<int> status);
        List<WorkflowTaskPoco> GetTasksAndGroupByInstanceId(Guid guid);
    }
}
