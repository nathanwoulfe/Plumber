using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Repositories.Interfaces
{
    public interface ITasksRepository
    {
        void InsertTask(WorkflowTaskInstancePoco poco);
        void UpdateTask(WorkflowTaskInstancePoco poco);

        int CountGroupTasks(int groupId);
        int CountPendingTasks();

        IEnumerable<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId);
        IEnumerable<WorkflowTaskInstancePoco> GetPendingTasks(IEnumerable<int> status);

        List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest);
        List<WorkflowTaskInstancePoco> GetFilteredPagedTasksForDateRange(DateTime oldest, string filter);

        List<WorkflowTaskInstancePoco> GetTasksByNodeId(int nodeId);

        List<WorkflowTaskInstancePoco> GetTaskSubmissionsForUser(int id, IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetTasksAndGroupByInstanceId(Guid guid);
    }
}
