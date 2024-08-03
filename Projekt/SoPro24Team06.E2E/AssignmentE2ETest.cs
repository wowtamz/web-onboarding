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
            var options = new ChromeOptions();
            //options.AddArguments("--headless");
            options.AddArguments("--disable-dev-shm-usage");
            options.AddArguments("--no-sandbox");
            options.AddArguments("--ignore-certificate-errors");
            options.AddArguments("--allow-insecure-localhost");
            options.AddArguments("--window-size=1920,1080");

            var service = ChromeDriverService.CreateDefaultService();
            _driver = new ChromeDriver(service, options);

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30)); // Increase wait time
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

        [Fact]
        public void StartProcessFromProcessTemplate()
        {
            _driver.Navigate().GoToUrl(baseurl);

            // Login
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

            // Go to ProcessTemplates
            _driver.Navigate().GoToUrl(baseurl + "ProcessTemplate/");
            if (_driver.Title.Contains("Prozesse"))
            {
                try
                {
                    IWebElement linkElement = _wait.Until(driver =>
                    {
                        return driver.FindElement(
                            By.XPath("//a[contains(@href, '/Process/Start/')]")
                        );
                    });

                    linkElement.Click();
                }
                catch (WebDriverTimeoutException exception)
                {
                    throw new Exception(
                        "WedDriver timedout during ProcessTemplate View" + exception.Message
                    );
                }
            }

            string processTitle = "";

            // Check if at Process Start
            if (_driver.Title.Contains("Vorgang starten"))
            {
                try
                {
                    IWebElement inputTitle = _wait.Until(driver =>
                    {
                        IWebElement element = driver.FindElement(By.Id("Title"));

                        return element.Displayed ? element : null;
                    });

                    if (inputTitle != null)
                    {
                        processTitle = inputTitle.GetAttribute("value");
                    }

                    // Select WorkerOfReference
                    IWebElement dropdownWorkerOfRef = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("workerOfRefDropdown")
                        )
                    );
                    SelectElement selectWorkerOfRef = new SelectElement(dropdownWorkerOfRef);
                    selectWorkerOfRef.SelectByIndex(1);

                    // Select ContractOfRefWorker
                    IWebElement dropdownContract = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("contractDropdown")
                        )
                    );
                    SelectElement selectContract = new SelectElement(dropdownContract);
                    selectContract.SelectByIndex(1);

                    // Select DepartmentOfRefWorker
                    IWebElement dropdownDepartment = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("departmentDropdown")
                        )
                    );
                    SelectElement selectDepartment = new SelectElement(dropdownDepartment);
                    selectDepartment.SelectByIndex(1);

                    // Click Start Button
                    IWebElement startButton = _wait.Until(driver =>
                    {
                        var buttons = driver.FindElements(By.TagName("button"));
                        IWebElement button = buttons.FirstOrDefault(b =>
                            b.Text.Equals("Starten", StringComparison.OrdinalIgnoreCase)
                        );

                        return button;
                    });

                    startButton.Click();
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Element not found.");
                }
                catch (WebDriverTimeoutException exception)
                {
                    throw new Exception(
                        "WedDriver timedout during Process Start View" + exception.Message
                    );
                }
            }

            _driver.Navigate().GoToUrl(baseurl + "Process");
            if (_driver.Title.Contains("Vorgänge"))
            {
                try
                {
                    IWebElement tdElement = _wait.Until(driver =>
                    {
                        IWebElement element = driver.FindElement(
                            By.XPath($"//td[text()='{processTitle}']")
                        );
                        return element.Displayed ? element : null;
                    });

                    Assert.NotNull(tdElement);
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Element not found.");
                }
                catch (WebDriverTimeoutException exception)
                {
                    throw new Exception(
                        "WedDriver timedout during Process View" + exception.Message
                    );
                }
            }
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
