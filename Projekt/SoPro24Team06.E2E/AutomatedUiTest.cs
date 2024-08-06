//-------------------------
// Author: Kevin Tornquist
//-------------------------

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
            options.AddArguments(
                "--no-sandbox",
                "--headless",
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--disable-extensions",
                "--disable-infobars",
                "--remote-debugging-port=9222",
                "--window-size=2560,1440",
                "enable-automation",
                "--disable-browser-side-navigation",
                "--ignore-certificate-errors"
            );

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
        public void CreateProcessTemplateWithAssignments()
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

                Console.WriteLine("DEBUG: Creating assignments...");
                for (int i = 0; i < 3; i++)
                {
                    var createAssignmentButton = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                            By.CssSelector("[aria-label='create-assignment-button']")
                        )
                    );
                    createAssignmentButton.Click();

                    var assignmentTitle = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Title")
                        )
                    );
                    assignmentTitle.SendKeys($"Assignment {i}");

                    /*  var assignmentDescription = _wait.Until(
                         SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                             By.Id("Description")
                         )
                     );
                     assignmentDescription.SendKeys($"Assignment Description {i}"); */

                    var assignmentRolesSelect = new SelectElement(
                        _wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                                By.Id("AssignedRole")
                            )
                        )
                    );
                    assignmentRolesSelect.SelectByIndex(i + 1);

                    var assignmentSaveButton = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                            By.CssSelector("[aria-label='save-assignment-button']")
                        )
                    );
                    assignmentSaveButton.Click();
                }

                var saveButton = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("[aria-label='save-process-template-button']")
                    )
                );

                saveButton.Click();
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new Exception(
                    "Failed to find an element during process template creation.",
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
