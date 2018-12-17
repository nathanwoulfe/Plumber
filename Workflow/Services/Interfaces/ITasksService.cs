using System;
using System.Collections.Generic;
using Workflow.Models;

namespace Workflow.Services.Interfaces
{
    public interface ITasksService
    {
        int CountPendingTasks();
        int CountGroupTasks(int groupId);

        List<WorkflowTaskViewModel> GetPendingTasks(IEnumerable<int> status, int count, int page);
        List<WorkflowTaskViewModel> GetAllGroupTasks(int groupId, int count, int page);
        List<WorkflowTaskViewModel> ConvertToWorkflowTaskList(List<WorkflowTaskPoco> taskInstances, bool sort = true, WorkflowInstancePoco instance = null);
        
        List<WorkflowTaskPoco> GetTasksWithGroupByInstanceGuid(Guid guid);
        List<WorkflowTaskPoco> GetAllPendingTasks(IEnumerable<int> status);
        List<WorkflowTaskPoco> GetTaskSubmissionsForUser(int id, IEnumerable<int> status);

        List<WorkflowTaskPoco> GetAllTasksForDateRange(DateTime oldest);
        List<WorkflowTaskViewModel> GetFilteredPagedTasksForDateRange(DateTime oldest, int? count, int? page, string filter = "");

        List<WorkflowTaskPoco> GetTasksByNodeId(int id);

        WorkflowTaskViewModel GetTask(int id);

        void InsertTask(WorkflowTaskPoco poco);
        void UpdateTask(WorkflowTaskPoco poco);
    }
}
