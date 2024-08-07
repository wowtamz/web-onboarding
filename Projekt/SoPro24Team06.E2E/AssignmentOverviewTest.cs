using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using Xunit;

//-------------------------
// Author of [Fact]: Michael Adolf
// Everything else: Jan Pfluger
//-------------------------

namespace SoPro24Team06.E2E
{
    /// <summary>
    /// Test class for the assignment overview page
    /// </summary>
    public class AssignmentOverviewTest
        : IClassFixture<CustomWebApplicationFactory<Program>>,
            IDisposable
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _webClient;
        private readonly ChromeDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly string _errorLocationClass = "AssignmentOverviewTest: ";
        private readonly Uri baseUrl = new Uri("https://localhost:7003/");

        public AssignmentOverviewTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            _factory = new CustomWebApplicationFactory<Program>();
            _webClient = _factory.CreateDefaultClient();

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

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
        }

        /// <summary>
        /// Test the assignment overview page by checking if all assignments are displayed correctly
        /// after logging in as a personal user and selecting each process
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AssignmentOverview()
        {
            string errorLocationFunktion = "AssignmentOverview: ";
            HttpClient client = _factory.CreateDefaultClient();

            ApplicationUser personal;
            List<Process> processes;
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<ApplicationUser>
                >();
                var roleManager = scope.ServiceProvider.GetRequiredService<
                    RoleManager<ApplicationRole>
                >();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                await SetTestData(context, userManager, roleManager);
                personal = await userManager.FindByEmailAsync("personal@example.com");
                processes = context.Processes.Include(p => p.Assignments).ToList();
            }

            if (personal == null)
            {
                throw new Exception($"{_errorLocationClass}{errorLocationFunktion}no user found");
            }

            await Login(personal.Email, "Personal@123");

            _driver
                .Navigate()
                .GoToUrl(
                    "https://localhost:7003/Assignment/ChangeTable?currentList=AllAssignments"
                );
            if (!_driver.Url.Contains("Assignment"))
            {
                throw new Exception(
                    $"{_errorLocationClass}{errorLocationFunktion}could not navigate to Assignments"
                );
            }

            foreach (var process in processes)
            {
                IWebElement assignmentListBody = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("allAssignmentsBody")
                    )
                );
                Assert.True(assignmentListBody.Displayed, "Assignment list not displayed");

                Console.WriteLine(
                    $"Checking process: {process.Title} with {process.Assignments.Count} assignments"
                );

                IWebElement processDropdown = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.Id("processDropdown")
                    )
                );
                processDropdown.Click();
                SelectElement selectProcess = new SelectElement(processDropdown);
                var options = selectProcess.Options;
                foreach (var o in options)
                {
                    if (o.Text == process.Title)
                    {
                        _wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(o)
                        );
                        o.Click();
                        _driver.ExecuteJavaScript(
                            "document.getElementById('assingmentFilterForm').submit();"
                        );
                        break;
                    }
                }
                assignmentListBody = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(
                        By.Id("allAssignmentsBody")
                    )
                );
                List<IWebElement> assignmentListContent = assignmentListBody
                    .FindElements(By.XPath(".//tr"))
                    .ToList();

                Console.WriteLine(
                    $"Found {assignmentListContent.Count} assignments in the table for process {process.Title}"
                );

                var assignmentsForProcess = process.Assignments;
                Assert.Equal(assignmentsForProcess.Count, assignmentListContent.Count);

                foreach (var assignment in assignmentsForProcess)
                {
                    bool assignmentFound = assignmentListContent.Any(row =>
                        row.FindElement(By.XPath(".//td[1]")).Text == assignment.Title
                    );
                    Console.WriteLine($"Assignment '{assignment.Title}' found: {assignmentFound}");
                    Assert.Contains(
                        assignmentListContent,
                        row => row.FindElement(By.XPath(".//td[1]")).Text == assignment.Title
                    );
                }
            }
        }

        /// <summary>
        /// Logs in the user with the given username and password and navigates to the home page
        /// </summary>
        public async Task Login(string userName, string password)
        {
            string errorLocationFunktion = "Login";
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
                    emailElement.SendKeys(userName);

                    var passwordElement = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Input_Password")
                        )
                    );
                    passwordElement.SendKeys(password);

                    IWebElement loginButton = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                            By.CssSelector("button[aria-label='login-submit']")
                        )
                    );
                    loginButton.Click();

                    if (_driver.Url.Contains("Identity/Account/Login"))
                    {
                        throw new Exception(
                            $"{_errorLocationClass}{errorLocationFunktion}Login unsuccessful"
                        );
                    }
                }
                catch (WebDriverTimeoutException ex)
                {
                    throw new Exception(
                        $"{_errorLocationClass}{errorLocationFunktion}Failed to find an element during login. {ex.Message}"
                    );
                }
            }

            if (_driver.Url.Contains("Identity/Account/Login"))
            {
                throw new Exception(
                    $"{_errorLocationClass}{errorLocationFunktion}Login unsuccessful"
                );
            }
        }

        /// <summary>
        /// Sets test data in the database for the assignment overview test
        /// </summary>
        public async Task SetTestData(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager
        )
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            string[] roleNames = { "Administrator", "User", "Personal" };
            foreach (var roleName in roleNames)
            {
                ApplicationRole role = new ApplicationRole(roleName);
                if (await roleManager.RoleExistsAsync(role.Name) == false)
                    await roleManager.CreateAsync(role);
                await context.SaveChangesAsync();
            }

            ApplicationUser admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FullName = "Admin User"
            };
            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Administrator");
            }

            ApplicationUser user = new ApplicationUser
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                FullName = "User User"
            };
            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                await userManager.CreateAsync(user, "User@123");
                await userManager.AddToRoleAsync(user, "User");
            }
            ApplicationUser personal = new ApplicationUser
            {
                UserName = "personal@example.com",
                Email = "personal@example.com",
                FullName = "Personal User"
            };
            if (await userManager.FindByEmailAsync(personal.Email) == null)
            {
                await userManager.CreateAsync(personal, "Personal@123");
                await userManager.AddToRoleAsync(personal, "Personal");
            }

            context.Contracts.Add(new Contract("Test"));
            context.Departments.Add(new Department("Test"));
            context.SaveChanges();
            context.DueTimes.AddRange(
                new DueTime("dueTime1", 1, 0, 0),
                new DueTime("dueTime2", 2, 0, 0),
                new DueTime("dueTime3", -2, 0, 0)
            );

            List<Assignment> assignments1 = new List<Assignment>
            {
                new AssignmentTemplate
                {
                    Title = "TestAssignment1",
                    Instructions = "Test",
                    AssigneeType = Enums.AssigneeType.ROLES,
                    AssignedRole = await roleManager.FindByNameAsync("Personal"),
                    DueIn = context.DueTimes.Find(1),
                    ProcessTemplateId = 0
                }.ToAssignment(personal, DateTime.Today),
                new AssignmentTemplate
                {
                    Title = "TestAssignment2",
                    Instructions = "Test",
                    AssigneeType = Enums.AssigneeType.SUPERVISOR,
                    DueIn = context.DueTimes.Find(2),
                    ProcessTemplateId = 0
                }.ToAssignment(personal, DateTime.Today)
            };

            List<Assignment> assignments2 = new List<Assignment>
            {
                new AssignmentTemplate
                {
                    Title = "TestAssignment3",
                    Instructions = "Test",
                    AssigneeType = Enums.AssigneeType.WORKER_OF_REF,
                    DueIn = context.DueTimes.Find(1),
                    ProcessTemplateId = 0
                }.ToAssignment(user, DateTime.Today),
                new AssignmentTemplate
                {
                    Title = "TestAssignment4",
                    Instructions = "Test",
                    AssigneeType = Enums.AssigneeType.WORKER_OF_REF,
                    DueIn = context.DueTimes.Find(3),
                    ProcessTemplateId = 0
                }.ToAssignment(user, DateTime.Today)
            };

            await context.Assignments.AddRangeAsync(assignments1);
            await context.Assignments.AddRangeAsync(assignments2);
            await context.SaveChangesAsync();

            List<Process> processes = new List<Process>
            {
                new Process(
                    "TestProcess1",
                    "Test",
                    assignments1,
                    user,
                    personal,
                    context.Contracts.First(),
                    context.Departments.First()
                ),
                new Process(
                    "TestProcess2",
                    "Test",
                    assignments2,
                    user,
                    personal,
                    context.Contracts.First(),
                    context.Departments.First()
                )
            };

            await context.Processes.AddRangeAsync(processes);
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _driver.Close();
            _driver.Dispose();
            _webClient.Dispose();
            _factory.Dispose();
        }
    }
}
