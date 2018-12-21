using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Workflow.Tests
{
    public class ChromeDriverFixture : IDisposable
    {
        public readonly ChromeDriver Driver;

        public const string BaseUrl = "http://localhost:56565/umbraco";
        public const string GroupsDashUrl = BaseUrl + "#/workflow/workflow/approval-groups/info";

        public const string EditorUser = "EditorUser@mail.com";
        public const string AdminUser = "AdminUser@mail.com";
        public const string EditorPassword = "JOP{H#kG";
        public const string AdminPassword = "tzX)TSiA";


        public ChromeDriverFixture()
        {
            Driver = new ChromeDriver
            {
                Url = BaseUrl
            };

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            Driver.Manage().Window.Maximize();
        }

        public void Login(string user, string pass)
        {
            Driver.FindElement(By.Name("username")).Clear();
            Driver.FindElement(By.Name("username")).SendKeys(user);

            Driver.FindElement(By.Name("password")).Clear();
            Driver.FindElement(By.Name("password")).SendKeys(pass);

            Driver.FindElement(By.CssSelector("[type='submit']")).Click();
        }

        /// <summary>
        /// Logout of the backoffice
        /// </summary>
        /// <param name="sleep">If the test is part of a theory, sleep after logout before logging back in</param>
        public void Logout(bool sleep = true)
        {
            DataElement("section-user").Click();
            DataElement("button-logOut").Click();
        }

        public void Wait(string css)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementExists(By.CssSelector(css)));
        }

        public IWebElement DataElement(string selector, string cascade = "")
        {
            return Driver.FindElement(By.CssSelector($"[data-element='{selector}'] {cascade}"));
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
