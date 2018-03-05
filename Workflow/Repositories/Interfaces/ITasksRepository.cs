using System;
using System.Collections.Generic;
using Workflow.Models;
using Workflow.UnitOfWork;

namespace Workflow.Repositories.Interfaces
{
    public interface ITasksRepository
    {
        void InsertTask(IUnitOfWork uow, WorkflowTaskInstancePoco poco);
        void UpdateTask(IUnitOfWork uow, WorkflowTaskInstancePoco poco);

        int CountGroupTasks(int groupId);
        int CountPendingTasks();

        List<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId, int count, int page);
        List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest);
        List<WorkflowTaskInstancePoco> GetPendingTasks(IEnumerable<int> status, int count, int page);
        List<WorkflowTaskInstancePoco> GetPendingTasks(IUnitOfWork uow, IEnumerable<int> status, int count, int page);
        List<WorkflowTaskInstancePoco> GetTasksByNodeId(int nodeId);

        List<WorkflowTaskInstancePoco> GetTaskSubmissionsForUser(IUnitOfWork uow, int id, IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetTasksAndGroupByInstanceId(IUnitOfWork uow, Guid guid);
    }
}
