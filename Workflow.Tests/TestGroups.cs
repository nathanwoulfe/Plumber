using System.Collections.Generic;
using System.Threading.Tasks;
using GDev.Umbraco.Test;
using Moq;
using NUnit.Framework;
using Workflow.Models;
using Workflow.Services.Interfaces;

namespace Workflow.Tests
{
    [TestFixture]
    public class TestGroups
    {
        private ContextMocker _mocker;
        private Mock<IGroupService> _groupService;

        [SetUp]
        public void Setup()
        {
            _groupService = new Mock<IGroupService>();
            _mocker = new ContextMocker();
        }

        [Test]
        public void CanGetAllGroups()
        {
            Task<IEnumerable<UserGroupPoco>> result = null;

            _groupService.Setup(m => m.GetUserGroupsAsync())
                .Returns(result);

            _groupService.Verify();

            Assert.IsNotNull(result, "No groups returned");
        }
    }
}
