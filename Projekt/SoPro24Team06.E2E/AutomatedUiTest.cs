using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using Xunit;

namespace SoPro24Team06.E2E
{
    public class AutomatedUiTest : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _webClient;

        public AutomatedUiTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            _factory = new CustomWebApplicationFactory<Program>();
            _webClient = _factory.CreateDefaultClient();
            using (var scope = _factory.Services.CreateScope())
            {
                // Resolve the UserManager Role Manager and context from the scope
                var userManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<ApplicationUser>
                >();
                var roleManager = scope.ServiceProvider.GetRequiredService<
                    RoleManager<ApplicationRole>
                >();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                SeedData.Initialize(userManager, roleManager, context).Wait();
            }
            var options = new ChromeOptions();
            options.AddArguments("--headless");
            options.AddArguments("--disable-dev-shm-usage");
            options.AddArguments("--no-sandbox");
            options.AddArguments("--window-size=1920,1080");

            var service = ChromeDriverService.CreateDefaultService();
            _driver = new ChromeDriver(service, options);

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60)); // Increase wait time
        }

        public void Login()
        {
            _driver.Navigate().GoToUrl("https://localhost:7003/");

            if (_driver.Url.Contains("Identity/Account/Login"))
            {
                try
                {
                    var emailElement = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Input_Email")
                        )
                    );
                    emailElement.SendKeys("admin@gmx.de");

                    var passwordElement = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Input_Password")
                        )
                    );
                    passwordElement.SendKeys("Admin1!");

                    var loginButton = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                            By.CssSelector("[aria-label='login-submit']")
                        )
                    );
                    loginButton.Click();
                }
                catch (WebDriverTimeoutException ex)
                {
                    throw new Exception("Failed to find an element during login.", ex);
                }
            }
        }

        [Fact]
        public void CreateProcessTemplate()
        {
            Login();
            _driver.Navigate().GoToUrl("https://localhost:7003/ProcessTemplate/Create");

            try
            {
                var title = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("Title"))
                );
                title.SendKeys("Test Process Template");

                var description = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("Description")
                    )
                );
                description.SendKeys("Test Description");

                var contractSelect = new SelectElement(
                    _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Name("ContractOfRefWorkerId")
                        )
                    )
                );
                contractSelect.SelectByIndex(0);

                var departmentSelect = new SelectElement(
                    _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Name("DepartmentOfRefWorkerId")
                        )
                    )
                );
                departmentSelect.SelectByIndex(0);

                var rolesSelect = new SelectElement(
                    _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("SelectedRoles")
                        )
                    )
                );
                rolesSelect.SelectByIndex(0);

                var createButton = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("[aria-label='create-process-template-submit']")
                    )
                );
                createButton.Click();
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new Exception(
                    "Failed to find an element during process template creation.",
                    ex
                );
            }
        }

        //[Fact]
        public void DeleteProcessTemplate()
        {
            Login();
            _driver.Navigate().GoToUrl("https://localhost:7003/ProcessTemplate");

            try
            {
                var processTemplateTitleElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.XPath("//td[text()='Test Process Template']")
                    )
                );
                processTemplateTitleElement.Click();

                var deleteButton = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("[aria-label='delete-process-template-submit']")
                    )
                );
                deleteButton.Click();

                _driver.Navigate().GoToUrl("https://localhost:7003/ProcessTemplate");
                var processTemplateTitleElementsAfterDelete = _wait.Until(driver =>
                    driver.FindElements(By.XPath("//td[text()='Test Process Template']"))
                );
                Assert.Empty(processTemplateTitleElementsAfterDelete);
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new Exception(
                    "Failed to find an element during process template deletion.",
                    ex
                );
            }
        }

        public void Dispose()
        {
            _webClient.Dispose();
            _factory.Dispose();
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
