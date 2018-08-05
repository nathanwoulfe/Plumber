using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace Workflow.Tests.Integration
{
    /// <summary>
    /// Non-admin users should not have access to the workflow section or any related controls
    /// </summary>
    [Collection("ChromeDriver collection")]
    public class UserAccessTests
    {
        private readonly ChromeDriver _driver;
        private readonly ChromeDriverFixture _fixture;

        public UserAccessTests(ChromeDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _fixture = fixture;
        }

        [Theory(Skip = "Ignore Selenium tests")]
        [InlineData(ChromeDriverFixture.EditorUser, ChromeDriverFixture.EditorPassword, false)]
        [InlineData(ChromeDriverFixture.AdminUser, ChromeDriverFixture.AdminPassword, true)]
        public void UserTypeHasCorrectAccess(string user, string password, bool expected)
        {
            _fixture.Login(user, password);

            // check for workflow section in nav
            Exception ex = Record.Exception(() => _fixture.DataElement("section-workflow"));
            Assert.Equal(expected, ex == null);

            Thread.Sleep(500);

            // check for save and publish button
            _fixture.DataElement("section-content", "a").Click();
            _fixture.DataElement("tree-root", "~ ul li:first-child").Click();

            _fixture.Wait(".workflow-button-drawer");
            ex = Record.Exception(() => _driver.FindElement(By.CssSelector("[key='buttons_saveAndPublish']")));
            Assert.Equal(expected, ex == null);

            Thread.Sleep(500);

            // check for workflow config menu option
            _fixture.DataElement("section-content", "a").Click();

            // hover the context ... button
            var action = new Actions(_fixture.Driver);
            action.MoveToElement(_fixture.DataElement("tree-root", "~ ul li:first-child")).ContextClick().Build().Perform();

            Thread.Sleep(2500);

            ex = Record.Exception(() => _fixture.DataElement("action-workflowConfig"));
            Assert.Equal(expected, ex == null);

            // don't forget to logout...
            _fixture.Logout(!expected);
        }
    }
}

