using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras;
using Xunit;

namespace SoPro24Team06.E2E
{
    public class AssignmentUiTest : IDisposable
    {
        private readonly string baseurl = "https://localhost:7003/";
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public AssignmentUiTest(IWebDriver driver, WebDriverWait wait)
        {
            Environment.SetEnvironmentVariable("DISPLAY", ":99");

            var options = new ChromeOptions();
            options.AddArguments("--no-sandbox");
            options.AddArguments("--headless");
            options.AddArguments("--disable-gpu");
            options.AddArguments("--disable-dev-shm-usage");
            options.AddArguments("--disable-extensions");
            options.AddArguments("--disable-infobars");
            options.AddArguments("--remote-debugging-port=9222");

            var service = ChromeDriverService.CreateDefaultService();
            service.LogPath = "chromedriver.log";
            service.EnableVerboseLogging = true;

            _driver = new ChromeDriver(service, options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
        }

        public void LoginNormalUser()
        {
            _driver.Navigate().GoToUrl(baseurl);

            if (_driver.Url.Contains("Identity/Account/Login"))
            {
                try
                {
                    var emailElement = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Input_Email")
                        )
                    );
                    emailElement.SendKeys("user@example.com");

                    var passwordElement = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Input_Password")
                        )
                    );
                    passwordElement.SendKeys("User@123");

                    var loginButton = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                            By.CssSelector("[aria-label='login-submit']")
                        )
                    );
                    loginButton.Click();
                }
                catch (WebDriverTimeoutException exception)
                {
                    throw new Exception(
                        "WedDriver timedout during loginAttempt" + exception.Message
                    );
                }
            }
        }

        public void LoginAdmin() { }

        public void LoginHRManager() { }

        public void TestAssingmentListDisplay()
        {
            _driver.Navigate().GoToUrl(baseurl + "Assignment");
            try
            {
                //test if AssignmentList is displayed
                IWebElement assignmentList = _wait.Until(d =>
                    d.FindElement(By.Id("MyAssignmentsList"))
                );
                Assert.True(
                    assignmentList.Displayed,
                    "assignmentList myAssignmentsList not displayed"
                );
                //test if there are no Assignments to show
                var assignmentListBody = assignmentList.FindElement(By.TagName("tbody"));
                try
                {
                    assignmentListBody.FindElement(
                        By.XPath("//tr/td[text()='Keine Aufgaben vorhanden']")
                    );
                    throw new Exception("no Assignments shown");
                }
                catch (NoSuchElementException exception) { }
                ;

                //test if assignments are correctly formed
                var assignments = assignmentListBody.FindElements(By.TagName("tr"));

                foreach (var a in assignments)
                {
                    var assignmentDetails = a.FindElements(By.TagName("td"));
                }
            }
            catch (NoSuchElementException e)
            {
                throw new Exception(
                    "Error: MyAssignmentList not Displayed correctly: \n" + e.Message
                );
            }
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
