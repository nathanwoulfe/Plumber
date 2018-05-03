using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Workflow.Tests.WebDriver
{
    public class GenericTests
    {
        private readonly ChromeDriver _driver;

        public GenericTests()
        {
            _driver = new ChromeDriver
            {
                Url = "http://localhost:56565/umbraco"
            };
        }

        private bool Can_Get_Login_Page()
        {
            return _driver.Title.IndexOf("Umbraco") != -1;
        }

        private bool Can_Do_Log_In()
        {
            _driver.FindElementByName("username").SendKeys("nathan@nathanw.com.au");
            _driver.FindElementByName("password").SendKeys("Nu277wa5__umbraco");

            _driver.FindElementByTagName("button").Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
            wait.Until(ExpectedConditions.ElementExists(By.TagName("section")));

            return _driver.FindElementByTagName("body").Displayed;

        }

        [Fact]
        public void Can_Login()
        {
            Assert.True(Can_Get_Login_Page());
            Assert.True(Can_Do_Log_In());

            _driver.Dispose();
        }
    }
}
