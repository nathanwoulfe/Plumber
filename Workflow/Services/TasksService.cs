using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Workflow.Events.Args;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services.Interfaces;

namespace Workflow.Services
{
    public class TasksService : ITasksService
    {
        private readonly IConfigService _configService;
        private readonly IGroupService _groupService;
        private readonly ITasksRepository _tasksRepo;

        public static event EventHandler<TaskEventArgs> Created;
        public static event EventHandler<TaskEventArgs> Updated;

        public TasksService() : this(new TasksRepository(), new ConfigService(), new GroupService())
        {
        }

        private TasksService(ITasksRepository tasksRepo, IConfigService configService, IGroupService groupService)
        {
            _tasksRepo = tasksRepo;
            _configService = configService;
            _groupService = groupService;
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
        public List<WorkflowTaskViewModel> GetPendingTasks(IEnumerable<int> status, int count, int page)
        {
            IEnumerable<WorkflowTaskPoco> taskInstances = _tasksRepo.GetAllPendingTasks(status)
                .Where(x => x.WorkflowInstance.Active)
                .GroupBy(x => x.WorkflowInstanceGuid)
                .Select(x => x.First());

            List<WorkflowTaskViewModel> tasks = ConvertToWorkflowTaskList(taskInstances.Skip((page - 1) * count).Take(count).ToList());

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<WorkflowTaskViewModel> GetAllGroupTasks(int groupId, int count, int page)
        {
            IEnumerable<WorkflowTaskPoco> taskInstances = _tasksRepo.GetAllGroupTasks(groupId);
            List<WorkflowTaskViewModel> tasks = ConvertToWorkflowTaskList(taskInstances.Skip((page - 1) * count).Take(count).ToList());

            return tasks;
        }

        /// <summary>
        /// Converts a list of workflowTaskInstances into a list of UI-ready workflowTasks
        /// </summary>
        /// <param name="taskInstances"></param>
        /// <param name="sorted">Depending on the caller, the response may not be sorted</param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public List<WorkflowTaskViewModel> ConvertToWorkflowTaskList(List<WorkflowTaskPoco> taskInstances, bool sorted = true, WorkflowInstancePoco instance = null)
        {
            List<WorkflowTaskViewModel> workflowItems = new List<WorkflowTaskViewModel>();

            if (!taskInstances.Any()) return workflowItems;

            bool useInstanceFromTask = instance == null;

            foreach (WorkflowTaskPoco taskInstance in taskInstances)
            {
                instance = useInstanceFromTask ? taskInstance.WorkflowInstance : instance;

                // ignore workflows where node has been deleted
                if (instance.Node == null || instance.Node.Path.Contains(Constants.System.RecycleBinContentString))
                {
                    continue;
                }

                // if the instance has been rejected/resubmitted, get the most recent comment from that stage
                if (instance.WorkflowStatus.In(WorkflowStatus.Rejected, WorkflowStatus.Resubmitted))
                {

                }

                var item = new WorkflowTaskViewModel
                {
                    InstanceGuid = instance.Guid,
                    TaskId = taskInstance.Id,

                    Type = instance.WorkflowType.Description(instance.ScheduledDate),
                    TypeId = instance.Type,
                    CurrentStep = taskInstance.ApprovalStep,

                    Status = taskInstance.Status,
                    StatusName = taskInstance.StatusName,
                    CssStatus = taskInstance.StatusName.ToLower().Replace(' ', '-'),
                    InstanceStatus = instance.WorkflowStatus.ToString().ToLower(),

                    NodeId = instance.NodeId,
                    NodeName = instance.Node.Name,

                    RequestedById = instance.AuthorUserId,
                    RequestedBy = instance.AuthorUser?.Name,
                    RequestedOn = taskInstance.CreatedDate.ToFriendlyDate(),
                    Comment = taskInstance.Comment,
                    InstanceComment = instance.AuthorComment,

                    ApprovalGroupId = taskInstance.UserGroup?.GroupId,
                    ApprovalGroup = taskInstance.UserGroup?.Name,
                    CompletedBy = taskInstance.ActionedByUser?.Name,
                    CompletedOn = taskInstance.CompletedDate?.ToFriendlyDate(),

                    ActionedByAdmin = taskInstance.ActionedByAdmin,

                    Permissions = _configService.GetRecursivePermissionsForNode(instance.Node)
                };

                workflowItems.Add(item);
            }

            return sorted ? workflowItems.OrderByDescending(x => x.CurrentStep).ToList() : workflowItems.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowTaskPoco> GetAllPendingTasks(IEnumerable<int> status)
        {
            IEnumerable<WorkflowTaskPoco> taskInstances = _tasksRepo.GetAllPendingTasks(status);

            return taskInstances
                .Where(x => x.WorkflowInstance.Active)
                .OrderByDescending(x => x.Id)
                .GroupBy(x => x.WorkflowInstanceGuid)
                .Select(x => x.First())
                .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <returns></returns>
        public List<WorkflowTaskPoco> GetAllTasksForDateRange(DateTime oldest)
        {
            List<WorkflowTaskPoco> taskInstances = _tasksRepo.GetAllTasksForDateRange(oldest);
            return taskInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldest"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<WorkflowTaskViewModel> GetFilteredPagedTasksForDateRange(DateTime oldest, int? count, int? page, string filter = "")
        {
            List<WorkflowTaskPoco> taskInstances = _tasksRepo.GetFilteredPagedTasksForDateRange(oldest, filter);

            // todo - fetch only required data, don't do paging here
            List<WorkflowTaskViewModel> workflowTaskInstances = ConvertToWorkflowTaskList(
                page.HasValue && count.HasValue ?
                    taskInstances.Skip((page.Value - 1) * count.Value).Take(count.Value).ToList() :
                    taskInstances);

            return workflowTaskInstances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<WorkflowTaskPoco> GetTasksByNodeId(int id)
        {
            List<WorkflowTaskPoco> taskInstances = _tasksRepo.GetTasksByNodeId(id);
            return taskInstances;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<WorkflowTaskPoco> GetTaskSubmissionsForUser(int id, IEnumerable<int> status)
        {
            return _tasksRepo.GetTaskSubmissionsForUser(id, status)
                .Where(x => x.WorkflowInstance.Active)
                .OrderByDescending(x => x.Id)
                .GroupBy(x => x.WorkflowInstanceGuid)
                .Select(x => x.First())
                .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<WorkflowTaskPoco> GetTasksWithGroupByInstanceGuid(Guid guid)
        {
            List<WorkflowTaskPoco> tasks = _tasksRepo.GetTasksAndGroupByInstanceId(guid);
            foreach (WorkflowTaskPoco task in tasks)
            {
                task.UserGroup = _groupService.GetPopulatedUserGroupAsync(task.GroupId).Result;
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WorkflowTaskViewModel GetTask(int id)
        {
            WorkflowTaskPoco task =_tasksRepo.Get(id);
            return ConvertToWorkflowTaskList(task.AsEnumerableOfOne().ToList()).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        public void InsertTask(WorkflowTaskPoco poco)
        {
            _tasksRepo.InsertTask(poco);
            Created?.Invoke(this, new TaskEventArgs(poco));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poco"></param>
        /// <returns></returns>
        public void UpdateTask(WorkflowTaskPoco poco)
        {
            _tasksRepo.UpdateTask(poco);
            Updated?.Invoke(this, new TaskEventArgs(poco));
        }
    }
}