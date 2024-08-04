using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.E2E
{
    public class AssignmentE2ETest
        : IClassFixture<CustomWebApplicationFactory<Program>>,
            IDisposable
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _webClient;
        private readonly ChromeDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly string _errorLocationClass = "AssignmentE2ETest";
        private readonly Uri baseUrl = new Uri("https://localhost:/7003/");
        private readonly string baseurl = "https://localhost:7003/";

        public AssignmentE2ETest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            _factory = new CustomWebApplicationFactory<Program>();
            _webClient = _factory.CreateDefaultClient();
            Environment.SetEnvironmentVariable("DISPLAY", ":99");

            var options = new ChromeOptions();
            options.AddArguments("--no-sandbox");
            options.AddArguments("--headless");
            options.AddArguments("--disable-gpu");
            options.AddArguments("--disable-dev-shm-usage");
            options.AddArguments("--disable-extensions");
            options.AddArguments("--disable-infobars");
            options.AddArguments("--remote-debugging-port=9222");
            options.AddArguments("--window-size=1920,1080");

            var service = ChromeDriverService.CreateDefaultService();
            _driver = new ChromeDriver(service, options);

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60)); // Increase wait time
        }

        [Fact]
        public async Task AssignmentListTest()
        {
            string errorLocationFunktion = "AssignmentListTest";
            //start Client
            HttpClient client = _factory.CreateDefaultClient();

            List<Assignment> assignments = new List<Assignment>();
            ApplicationUser user;
            //using scoped services to create only neccecary Data in Database
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

                await SetTestData(context, userManager, roleManager);
                user = await userManager.FindByEmailAsync("user@example.com");
            }
            if (user == null)
                throw new Exception("no user found");
            //login as Normal User
            await Login("user@example.com", "User@123");

            //check Assignment List:
            _driver.Navigate().GoToUrl("https://localhost:7003/Assignment/");

            //test if AssignmentList is displayed
            IWebElement assignmentListBody = _wait.Until(d =>
                d.FindElement(By.Id("MyAssignments"))
            );
            try
            {
                Assert.True(
                    assignmentListBody.Displayed,
                    "assignmentList myAssignmentsList not displayed"
                );
            }
            catch (Exception e)
            {
                throw new Exception(
                    ""
                        + _errorLocationClass
                        + errorLocationFunktion
                        + "AssignmentList-isDisplayed-Check: "
                        + e.Message
                );
            }

            //check if any Assignments are Displayed
            try
            {
                assignmentListBody.FindElement(
                    By.XPath("//tr/td[text()='Keine Aufgaben vorhanden']")
                );

                throw new Exception(
                    ""
                        + _errorLocationClass
                        + errorLocationFunktion
                        + "AssignmentList-noAssignments-Check: noAssignmentsFound"
                );
            }
            catch (Exception e) { }

            //filter assignments for User
            List<Assignment> assignmentsForUser = assignments
                .Where(a => a.Assignee != null && a.Assignee == user)
                .ToList();

            //check number of Displayed Elements:
            List<IWebElement> assignmentListContent = assignmentListBody
                .FindElements(By.XPath("/tr"))
                .ToList();
            if (assignmentListContent.Count() != assignmentsForUser.Count)
            {
                throw new Exception(
                    ""
                        + _errorLocationClass
                        + errorLocationFunktion
                        + "AssignmentList-NumberOfAssignments-Check: is not correct"
                );
            }

            //check content of Assignments
            foreach (var a in assignmentListContent)
            {
                IWebElement TitleElement = a.FindElement(By.XPath("/td[1]"));
                string assignmentTitle = TitleElement.Text;
                //check if Assignment is actually in the List
                Assignment? assignmentToCheck = assignmentsForUser.Find(a =>
                    a.Title == assignmentTitle
                );
                if (assignmentToCheck != null)
                {
                    //check if Assignment is overdue
                    if (assignmentToCheck.DueDate.CompareTo(DateTime.Today) < 0)
                    {
                        if (a.GetAttribute("class") != ".overdue")
                        {
                            throw new Exception(
                                ""
                                    + _errorLocationClass
                                    + errorLocationFunktion
                                    + "AssignmentList-CheckListContent: overDue Assignment not Displayed Correctly"
                            );
                        }
                    }

                    //check if DaysTillDueDate are displayed Correctly:
                    if (
                        a.FindElement(By.XPath("/td[3]")).Text
                        != assignmentToCheck.GetDaysTillDueDate().ToString()
                    )
                    {
                        throw new Exception(
                            ""
                                + _errorLocationClass
                                + errorLocationFunktion
                                + "AssignmentList-CheckListContent: Days Till DueDate not displayed correctly"
                        );
                    }

                    assignmentsForUser.Remove(assignmentToCheck);
                }
                else
                {
                    throw new Exception(
                        ""
                            + _errorLocationClass
                            + errorLocationFunktion
                            + "AssignmentList-CheckListContent: An Assignment was not found in the List for the current User"
                    );
                }

                if (assignmentsForUser.Count() != 0)
                {
                    throw new Exception(
                        ""
                            + _errorLocationClass
                            + errorLocationFunktion
                            + "AssignmentList-CheckListContent: not all Assignments from the Database where Displayed"
                    );
                }
            }
        }

        public async Task Login(string userName, string password)
        {
            _driver.Navigate().GoToUrl("https://localhost:7003/");

            if (_driver.Url.Contains("Identity/Account/Login"))
            {
                try
                {
                    try
                    {
                        var emailElement = _wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                                By.Id("Input_Email")
                            )
                        );
                        emailElement.SendKeys(userName);
                    }
                    catch
                    {
                        throw new Exception("Failed to find userName Field");
                    }

                    try
                    {
                        var passwordElement = _wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                                By.Id("Input_Password")
                            )
                        );
                        passwordElement.SendKeys(password);
                    }
                    catch
                    {
                        throw new Exception("Failed to find Password Field");
                    }

                    IWebElement loginButton;
                    try
                    {
                        loginButton = _wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                                By.CssSelector("button[aria-label='login-submit']")
                            )
                        );
                    }
                    catch
                    {
                        throw new Exception("Failed to find Login Button Field");
                    }

                    bool isElementObscured = (bool)
                        ((IJavaScriptExecutor)_driver).ExecuteScript(
                            "return !(arguments[0].getBoundingClientRect().top >= 0 && arguments[0].getBoundingClientRect().bottom <= (window.innerHeight || document.documentElement.clientHeight));",
                            loginButton
                        );

                    if (isElementObscured)
                    {
                        throw new Exception("Login button is obscured by another element.");
                    }

                    try
                    {
                        loginButton.Click();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Failed to click login button" + e.Message);
                    }

                    if (_driver.Url.Contains("Identity/Account/Login"))
                    {
                        throw new Exception("not redirected to Assignment Screen");
                    }
                }
                catch (WebDriverTimeoutException ex)
                {
                    throw new Exception("Failed to find an element during login.", ex);
                }
            }
        }

        public async Task SetTestData(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager
        )
        {
            //clear database to ensure an empty database
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            //insert Required Roles:
            string[] roleNames = { "Administrator", "User", "Personal" };
            foreach (var roleName in roleNames)
            {
                ApplicationRole role = new ApplicationRole(roleName);
                if (await roleManager.RoleExistsAsync(role.Name) == false)
                    await roleManager.CreateAsync(role);
                await context.SaveChangesAsync();
            }

            //insert Required Users
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
            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                await userManager.CreateAsync(personal, "Personal@123");
                await userManager.AddToRoleAsync(personal, "Personal");
            }
            //create Deparments and Contracts
            context.Contracts.Add(new Contract("Test"));
            context.Departments.Add(new Department("Test"));
            context.SaveChanges();
            //create DueTime
            context.DueTimes.AddRange(
                new DueTime("dueTime1", 1, 0, 0),
                new DueTime("dueTime2", 2, 0, 0),
                new DueTime("dueTime3", -2, 0, 0)
            );
            //create Assignments
            DateTime DueDate = DateTime.Today;
            List<Assignment> assignmens =
                new()
                {
                    new AssignmentTemplate
                    {
                        Title = "TestRolePersonal",
                        Instructions = "Test",
                        AssigneeType = Enums.AssigneeType.ROLES,
                        AssignedRole = await roleManager.FindByNameAsync("Personal"),
                        DueIn = context.DueTimes.Find(1),
                        ProcessTemplateId = 0
                    }.ToAssignment(personal, DateTime.Today),
                    new AssignmentTemplate
                    {
                        Title = "TestUserPersonal",
                        Instructions = "Test",
                        AssigneeType = Enums.AssigneeType.SUPERVISOR,
                        DueIn = context.DueTimes.Find(2),
                        ProcessTemplateId = 0
                    }.ToAssignment(personal, DateTime.Today),
                    new AssignmentTemplate
                    {
                        Title = "TestUserUser",
                        Instructions = "Test",
                        AssigneeType = Enums.AssigneeType.WORKER_OF_REF,
                        DueIn = context.DueTimes.Find(1),
                        ProcessTemplateId = 0
                    }.ToAssignment(user, DateTime.Today),
                    new AssignmentTemplate
                    {
                        Title = "TestUserUserOverDue",
                        Instructions = "Test",
                        AssigneeType = Enums.AssigneeType.WORKER_OF_REF,
                        DueIn = context.DueTimes.Find(3),
                        ProcessTemplateId = 0
                    }.ToAssignment(user, DateTime.Today)
                };
            await context.Assignments.AddRangeAsync(assignmens);
            await context.SaveChangesAsync();

            List<Process> processes = new List<Process>
            {
                new Process(
                    "Test",
                    "Test",
                    context.Assignments.ToList(),
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
