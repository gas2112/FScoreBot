using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace FScoreBot
{
    public class SeleniumWrapper
    {
        private IWebDriver Driver { get; set; }
        private string WebDriverDirectory { get; set; }

        public SeleniumWrapper(string webdriverdirectory)
        {
            WebDriverDirectory = webdriverdirectory;
        }

        public bool OpenBrowser()
        {
            try
            {
                Driver = new ChromeDriver(WebDriverDirectory);
                Driver.Manage().Window.Maximize();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CloseBrowserAndDriver()
        {
            try
            {
                Driver.Close();
                Driver.Quit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void CloseLastWindow()
        {
            Driver.Close();
            Driver.SwitchTo().Window(Driver.WindowHandles.Last());
        }

        public List<IWebElement> FindElementsByClass(string className)
        {
            return Driver.FindElements(By.ClassName(className)).ToList();
        }

        public IWebElement FindElementById(string id)
        {
            return Driver.FindElement(By.Id(id));
        }

        public bool ClickById(string id)
        {
            try
            {
                Driver.FindElement(By.Id(id)).Click();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ClickAndChangeWindow(IWebElement webElement)
        {
            webElement.Click();
            Driver.SwitchTo().Window(Driver.WindowHandles.Last());
        }

        public void GoToUrl(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }
    }
}
