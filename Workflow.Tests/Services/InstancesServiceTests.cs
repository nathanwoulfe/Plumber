using System;
using System.Collections.Generic;
using System.Linq;
using Chauffeur.TestingTools;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Workflow.Models;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Services
{
    public class InstancesServiceTests : UmbracoHostTestBase
    {
        private readonly IInstancesService _service;
        private readonly ITasksService _tasksService;

        public InstancesServiceTests()
        {
            Host.Run(new[] {"install y"}).Wait();

            Scaffold.Run();
            Scaffold.Config();
            Scaffold.ContentType(ApplicationContext.Current.Services.ContentTypeService);

            _service = new InstancesService();
            _tasksService = new TasksService();
        }

        [Theory]
        [InlineData(95)]
        [InlineData(3)]
        [InlineData(20)]
        public void Can_Get_All(int count)
        {
            Scaffold.Instances(count);
            IEnumerable<WorkflowInstancePoco> instances = _service.GetAll();

            Assert.Equal(count, instances.Count());
        }


        [Theory]
        [InlineData(65)]
        [InlineData(8)]
        [InlineData(11)]
        public void Can_Count_All(int count)
        {
            Scaffold.Instances(count);
            double result = _service.CountAll();

            Assert.Equal(count, result);
        }

        [Theory]
        [InlineData(4, 3)]
        [InlineData(1, 10)]
        [InlineData(3, 55)]
        public void Can_Count_Pending(int pendingCount, int approvedCount)
        {
            Scaffold.Instances(pendingCount);
            Scaffold.Instances(approvedCount, status: (int)WorkflowStatus.Approved);

            int result = _service.CountPending();

            Assert.Equal(pendingCount, result);
        }

        [Theory]
        [InlineData(1075, 2, 3)]
        [InlineData(9999, 11, 2)]
        [InlineData(1242, 43, 1)]
        public void Can_Get_For_Node_By_Status(int nodeId, int count, int status)
        {
            Scaffold.Instances(count, status: status, nodeId: nodeId);

            List<WorkflowInstancePoco> results = _service.GetForNodeByStatus(nodeId, new [] { status }).ToList();

            Assert.Equal(count, results.Count);
            foreach (WorkflowInstancePoco result in results)
            {
                Assert.Equal(nodeId, result.NodeId);
                Assert.Equal(status, result.Status);
            }
        }

        [Theory]
        [InlineData(1075, 2)]
        [InlineData(9999, 11)]
        [InlineData(1242, 43)]
        public void Can_Get_For_Node_By_Status_When_Status_Null(int nodeId, int count)
        {
            Scaffold.Instances(count, nodeId: nodeId);

            List<WorkflowInstancePoco> results = _service.GetForNodeByStatus(nodeId, null).ToList();

            Assert.Equal(count, results.Count);
            foreach (WorkflowInstancePoco result in results)
            {
                Assert.Equal(nodeId, result.NodeId);
            }
        }

        [Fact]
        public void Can_Get_All_Active_Instances()
        {
            int[] ids = {1051, 1052, 1063};

            foreach (int id in ids)
            {
                Scaffold.Instances(1, nodeId: id);
            }

            Dictionary<int, bool> results = _service.IsActive(ids);

            Assert.True(results[ids[0]]);
            Assert.True(results[ids[1]]);
            Assert.True(results[ids[2]]);

            // try again, but with two ids with no attached instance
            int[] moreIds = {1051, 3333, 4444};
            results = _service.IsActive(moreIds);

            Assert.True(results[moreIds[0]]);
            Assert.False(results[moreIds[1]]);
            Assert.False(results[moreIds[2]]);
        }
        
        [Theory]
        [InlineData(2, -3, 2)]
        [InlineData(11, -2, 11)]
        [InlineData(43, 1, 43)]
        public void Can_Get_For_Date_Range(int count, int daysAgo, int expected)
        {
            IEnumerable<WorkflowInstancePoco> instances = Scaffold.Instances(count);

            List<WorkflowInstance> results = _service.GetAllInstancesForDateRange(DateTime.Now.AddDays(daysAgo));

            // all instances are incomplete, so return regardless of daysAgo value
            Assert.Equal(expected, results.Count);

            foreach (WorkflowInstancePoco instance in instances)
            {
                instance.CompletedDate = DateTime.Now.AddDays(daysAgo - 10);
                _service.UpdateInstance(instance);
            }

            results = _service.GetAllInstancesForDateRange(DateTime.Now.AddDays(daysAgo));

            // all instances are now completed prior to the oldest date so should return an empty set
            Assert.Empty(results);

        }

        [Theory]
        [InlineData(23, -3, 21)]
        [InlineData(11, -2, 9)]
        [InlineData(43, 1, 41)]
        public void Can_Get_Filtered_Paged_For_Date_Range(int count, int daysAgo, int expected)
        {
            Scaffold.Instances(count);

            List<WorkflowInstance> results = _service.GetFilteredPagedInstancesForDateRange(DateTime.Now.AddDays(daysAgo), expected, 1, "3");

            // all instances are incomplete, so return regardless of daysAgo value
            Assert.Equal(expected, results.Count);
        }

        [Fact]
        public void Can_Update_Instance()
        {
            Guid guid = Guid.NewGuid();
            const string comment = "This here is an update";

            _service.InsertInstance(Scaffold.Instance(guid, 1));

            WorkflowInstancePoco instance = _service.GetByGuid(guid);

            instance.AuthorComment = comment;

            _service.UpdateInstance(instance);
            WorkflowInstancePoco updatedInstance = _service.GetByGuid(guid);

            Assert.Equal(comment, updatedInstance.AuthorComment);
        }

        /// <summary>
        /// Expected result is the count of items on the nth page
        /// </summary>
        /// <param name="instanceCount"></param>
        /// <param name="page"></param>
        /// <param name="count"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData(23, 4, 3, 3)]
        [InlineData(11, 3, 9, 0)]
        [InlineData(43, 2, 41, 2)]
        public void Can_Get_Paged(int instanceCount, int page, int count, int expected)
        {
            Scaffold.Instances(instanceCount);

            List<WorkflowInstance> instances = _service.Get(page, count);

            Assert.Equal(expected, instances.Count);
        }

        /// <summary>
        /// This relies on scaffolded config - to get populated groups on the tasks, it must come from config
        /// </summary>
        [Theory]
        [InlineData(4, 2)]
        [InlineData(3, 5)]
        [InlineData(2, 3)]
        public void Can_Get_Populated_Instance(int taskCount, int lastGroupId)
        {
            IContent node = Scaffold.Node(ApplicationContext.Current.Services.ContentService);

            Guid guid = Guid.NewGuid();

            WorkflowInstancePoco instance = Scaffold.Instance(guid, (int) WorkflowType.Publish, node.Id);

            _service.InsertInstance(instance);
            for (var i = 1; i <= taskCount; i += 1)
            {
                _tasksService.InsertTask(Scaffold.Task(guid, DateTime.Now, 
                    i < taskCount ? i : lastGroupId, 
                    i, 
                    i < taskCount ? 1 : 3));
            }

            // this has groups, tasks, everything. Or it should.
            WorkflowInstancePoco populatedInstance = _service.GetPopulatedInstance(guid);

            Assert.Equal(taskCount, populatedInstance.TaskInstances.Count);
            Assert.Equal(0, populatedInstance.TotalSteps); // this shouldn't be set yet
            Assert.Equal(lastGroupId, populatedInstance.TaskInstances.First().UserGroup.GroupId); // tasks are descending by id
            Assert.Equal(WorkflowStatus.PendingApproval, populatedInstance.WorkflowStatus);
        }

        [Fact]
        public void Can_Get_By_Guid()
        {
            var guid = Guid.NewGuid();

            _service.InsertInstance(Scaffold.Instance(guid, 1));

            WorkflowInstancePoco instance = _service.GetByGuid(guid);

            Assert.NotNull(instance);
            Assert.Equal(guid, instance.Guid);
        }

        [Fact]
        public void Can_Get_By_NodeId()
        {
            const int nodeId = 1075;

            _service.InsertInstance(Scaffold.Instance(Guid.NewGuid(), 1, nodeId));

            List<WorkflowInstance> instances = _service.GetByNodeId(nodeId, 1, 10);

            Assert.NotNull(instances);
            Assert.Equal(nodeId, instances.First().NodeId);
        }

        [Fact]
        public void Converting_Empty_Set_To_Instance_List_Returns_Empty_List()
        {
            List<WorkflowInstancePoco> instances = new List<WorkflowInstancePoco>();
            List<WorkflowInstance> result = _service.ConvertToWorkflowInstanceList(instances);

            Assert.Empty(result);
        }
    }
}
