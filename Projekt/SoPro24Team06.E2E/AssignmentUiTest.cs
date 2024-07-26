using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace SoPro24Team06.E2E
{
    public class AssignmentUiTest
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public AssignmentUiTest(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        public void LoginNormalUser()
        {
            _driver.Navigate().GoToUrl("https://localhost.7003/");

            if (_driver.Url.Contains("Identity/Account/Login"))
            {
                // try
                // {
                // 	var emailElement = _wait.Until(
                // 		SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("Input_Password"))
                // 	);
                // 	emailElement.SendKeys("user@example.com")

                // 	var passwordElement = _wait.Until(

                // 	)
                // }
                // catch (System.Exception)
                // {

                // 	throw;
                // }
            }
        }

        public void LoginAdmin() { }

        public void LoginHRManager() { }
    }
}
