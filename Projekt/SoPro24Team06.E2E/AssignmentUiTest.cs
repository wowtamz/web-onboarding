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

        public void Login() { }
    }
}
