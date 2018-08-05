using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace Workflow.Tests.Integration
{
    /// <summary>
    /// Ensure admin user can create, edit and delete user groups
    /// </summary>
    [Collection("ChromeDriver collection")]
    public class GroupTests
    {
        private readonly ChromeDriver _driver;
        private readonly ChromeDriverFixture _fixture;

        public GroupTests(ChromeDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _fixture = fixture;
        }

        [Fact(Skip = "Ignore Selenium tests"), Priority(1)]
        public void AdminCanAccessGroups()
        {
            _fixture.Login(ChromeDriverFixture.AdminUser, ChromeDriverFixture.AdminPassword);
            _driver.Url = ChromeDriverFixture.GroupsDashUrl;

            Thread.Sleep(500);

            Exception ex = Record.Exception(() => _driver.FindElement(By.CssSelector("form.workflow .umb-editor-container")));
            Assert.True(ex == null);
        }

        [Fact(Skip = "Ignore Selenium tests"), Priority(2)]
        public void AdminCanCreateGroup()
        {
            Thread.Sleep(500);

            // hover the context ... button
            var action = new Actions(_fixture.Driver);
            action.MoveToElement(_fixture.DataElement("tree-item-Approval groups", ".umb-options")).Click().Build().Perform();

            Thread.Sleep(5555555);
            _fixture.Logout(false);

        }
    }
}