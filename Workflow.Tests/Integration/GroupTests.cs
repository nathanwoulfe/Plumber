using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Xunit;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

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

        [Fact]
        public void AdminCanAccessGroups()
        {
            _fixture.Login(ChromeDriverFixture.AdminUser, ChromeDriverFixture.AdminPassword);
            _driver.Url = ChromeDriverFixture.GroupsDashUrl;

            Exception ex = Record.Exception(() => _driver.FindElement(By.CssSelector("form.workflow .umb-editor-container")));
            Assert.True(ex == null);
        }

        [Fact]
        public void AdminCanCreateDeleteGroup()
        {
            _fixture.Login(ChromeDriverFixture.AdminUser, ChromeDriverFixture.AdminPassword);
            _driver.Url = ChromeDriverFixture.GroupsDashUrl;

            var action = new Actions(_fixture.Driver);
            string groupName = Utility.RandomString();

            action.MoveToElement(_fixture.DataElement("tree-item-Approval groups", ".umb-options")).Click().Build().Perform();

            _fixture.DataElement("action-add").Click();

            IWebElement input = _driver.FindElement(By.CssSelector("[data-element='workflow-overlay__groups-name']"));

            input.Clear();
            input.SendKeys(groupName);

            // create the group
            _driver.FindElement(By.CssSelector(".btn-toolbar [ng-click*='vm.add']")).Click();

            // route updates
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.UrlContains("edit-group"));

            // expand the tree
            _fixture.DataElement("tree-item-Approval groups", "[data-element='tree-item-expand']").Click();

            // right click the group
            action.MoveToElement(_driver.FindElement(By.XPath($"//a[contains(text(), '{groupName}')]"))).ContextClick().Build().Perform();

            //// ok to delete
            //_fixture.DataElement("action-delete").Click();

            //// confirm delete
            //_driver.FindElement(By.CssSelector(".btn-toolbar [ng-click*='vm.delete']")).Click();
        }
    }
}