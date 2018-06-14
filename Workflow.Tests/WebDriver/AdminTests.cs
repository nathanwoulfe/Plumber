using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace Workflow.Tests.WebDriver
{
    [Collection("ChromeDriver collection")]
    public class AdminTests
    {
        private readonly ChromeDriver _driver;
        private readonly ChromeDriverFixture _fixture;

        public AdminTests(ChromeDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _fixture = fixture;

            fixture.AdminLogin();
        }

        [Fact]
        public void Admin_Can_Access_Workflow_Section()
        { 
            _fixture.Wait(".sections");

            IWebElement element = _driver.FindElement(By.CssSelector("li[data-element='section-workflow']"));
            Assert.NotNull(element);
        }

        [Fact]
        public void Admin_Can_Save_And_Publish()
        {
            _driver.Url = "http://localhost:56565/umbraco#/content/content/edit/1089";
            _fixture.Wait(".workflow-button-drawer");

            Assert.Throws<NoSuchElementException>(() => _driver.FindElementByLinkText("Save and publish"));
        }

    }
}

