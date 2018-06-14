using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Workflow.Tests.WebDriver
{
    [Collection("ChromeDriver collection")]
    public class EditorTests
    {
        private readonly ChromeDriver _driver;
        private readonly ChromeDriverFixture _fixture;

        public EditorTests(ChromeDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _fixture = fixture;

            fixture.EditorLogin();
        }

        [Fact]
        public void Editor_Cant_Access_Workflow_Section()
        {
            _fixture.Wait(".sections");

            Assert.Throws<NoSuchElementException>(() => 
                _driver.FindElement(By.CssSelector("li[data-element='section-workflow']")));
        }

        [Fact]
        public void Editor_Cant_Save_And_Publish()
        {
            _driver.Url = "http://localhost:56565/umbraco#/content/content/edit/1089";

            _fixture.Wait(".workflow-button-drawer");
            Assert.Throws<NoSuchElementException>(() => _driver.FindElementByLinkText("Save and publish"));
        }

    }
}

