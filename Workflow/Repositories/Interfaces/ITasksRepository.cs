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

        IEnumerable<WorkflowTaskInstancePoco> GetAllGroupTasks(int groupId);
        IEnumerable<WorkflowTaskInstancePoco> GetPendingTasks(IEnumerable<int> status);

        List<WorkflowTaskInstancePoco> GetAllPendingTasks(IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetAllTasksForDateRange(DateTime oldest);
        List<WorkflowTaskInstancePoco> GetTasksByNodeId(int nodeId);

        List<WorkflowTaskInstancePoco> GetTaskSubmissionsForUser(IUnitOfWork uow, int id, IEnumerable<int> status);
        List<WorkflowTaskInstancePoco> GetTasksAndGroupByInstanceId(IUnitOfWork uow, Guid guid);
    }
}
