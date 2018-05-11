using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Workflow.Tests.WebDriver
{
    public class ChromeDriverFixture : IDisposable
    {
        public readonly ChromeDriver Driver;

        private const string EditorUser = "EditorUser@mail.com";
        private const string AdminUser = "AdminUser@mail.com";
        private const string EditorPassword = "JOP{H#kG";
        private const string AdminPassword = "tzX)TSiA";

        public ChromeDriverFixture()
        {
            Driver = new ChromeDriver
            {
                Url = "http://localhost:56565/umbraco"
            };
        }

        private void Login(string user, string pass)
        {
            Driver.FindElementByName("username").SendKeys(user);
            Driver.FindElementByName("password").SendKeys(pass);

            Driver.FindElementByTagName("button").Click();

            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(2));
            wait.Until(ExpectedConditions.ElementExists(By.TagName("section")));
        }

        public void EditorLogin()
        {
            Login(EditorUser, EditorPassword);
        }

        public void AdminLogin()
        {
            Login(AdminUser, AdminPassword);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Driver.Dispose();
        }
    }

    [CollectionDefinition("ChromeDriver collection")]
    public class ChromeDriverCollection : ICollectionFixture<ChromeDriverFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
