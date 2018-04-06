using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Services.Interfaces
{
    public interface ITasksService
    {
        int CountPendingTasks();
        int CountGroupTasks(int groupId);

        List<WorkflowTask> GetPendingTasks(IEnumerable<int> status, int count, int page);
        List<WorkflowTask> GetAllGroupTasks(int groupId, int count, int page);
        List<WorkflowTask> ConvertToWorkflowTaskList(List<WorkflowTaskInstancePoco> taskInstances, bool sort = true, WorkflowInstancePoco instance = null);
        
        List<WorkflowTaskInstancePoco> GetTasksWithGroupByInstanceGuid(Guid guid);
        List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetTaskSubmissionsForUser(int id, IEnumerable<int> status);

        List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest);
        List<WorkflowTaskInstancePoco> GetTasksByNodeId(int id);

        void InsertTask(WorkflowTaskInstancePoco poco);
        void UpdateTask(WorkflowTaskInstancePoco poco);
    }
}
