using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.preview;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Services.Interfaces;
using TaskStatus = Workflow.Models.TaskStatus;

// Document and User are obsolete - disables Resharper warning
// alternative is to rebuild a local version of PreviewContent...
#pragma warning disable 618

namespace Workflow.Services
{
    public class PreviewService : IPreviewService
    {
        private readonly ITasksService _tasksService;
        private readonly IGroupService _groupService;

        public PreviewService()
            : this(new TasksService(), new GroupService())
        {
        }

        private PreviewService(ITasksService tasksService, IGroupService groupService)
        {
            _tasksService = tasksService;
            _groupService = groupService;
        }

        public void Generate(int nodeId, int userId, Guid workflowInstanceGuid)
        {
            var user = new User(userId);
            var d = new Document(nodeId);
            var pc = new PreviewContent(user, workflowInstanceGuid, false);

            pc.PrepareDocument(user, d, true);
            pc.SavePreviewSet();
            pc.ActivatePreviewCookie();
        }

        /// <summary>
        /// Delete from /app_plugins/workflow/preview
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="userId"></param>
        /// <param name="taskId"></param>
        /// <param name="guid"></param>
        public async Task<bool> Validate(int nodeId, int userId, int taskId, Guid guid)
        {
            List<WorkflowTaskPoco> taskInstances = _tasksService.GetTasksByNodeId(nodeId);

            if (!taskInstances.Any() || taskInstances.Last().TaskStatus == TaskStatus.Cancelled)
            {
                return false;
            }

            // only interested in last active task
            WorkflowTaskPoco activeTask = taskInstances.OrderBy(t => t.Id).LastOrDefault(t => t.TaskStatus.In(TaskStatus.PendingApproval, TaskStatus.Rejected));

            if (activeTask == null)
            {
                return false;
            }

            UserGroupPoco group = await _groupService.GetPopulatedUserGroupAsync(activeTask.GroupId);

            // only valid if the task belongs to the current workflow, and the user is in the current group, and the task id is correct
            return activeTask.WorkflowInstanceGuid == guid && group.Users.Any(u => u.UserId == userId) && activeTask.Id == taskId;
        }
    }
}
