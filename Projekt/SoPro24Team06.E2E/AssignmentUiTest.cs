//beginn code ownership Jan Pfluger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using Xunit;

namespace SoPro24Team06.E2E
{
    public class AssignmentUiTest : IDisposable
    {
        private readonly string _errorLocationClass = "AssignmentUiTest";
        private readonly string _baseUrl = "https://localhost:7003/";
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AssignmentUiTest()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-dev-shm-usage");
            chromeOptions.AddArguments("--no-sandbox");
            chromeOptions.AddArguments("--window-size=1920,1080");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            _driver = new ChromeDriver(chromeDriverService, chromeOptions);

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60)); // Increase wait time

            var services = new ServiceCollection();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("DataSource=:memory:")
            );
            services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            _serviceProvider = services.BuildServiceProvider();

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.OpenConnection();

                context.Database.EnsureCreated();
                _context = context;

                _userManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<ApplicationUser>
                >();
                _roleManager = scope.ServiceProvider.GetRequiredService<
                    RoleManager<ApplicationRole>
                >();
                SeedDatabase().Wait();
            }
        }

        public void Login()
        public void Login()
        {
            _driver.Navigate().GoToUrl("https://localhost:7003/");
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
                    emailElement.SendKeys("admin@example.com");
                    emailElement.SendKeys("admin@example.com");

                    var passwordElement = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Input_Password")
                        )
                    );
                    passwordElement.SendKeys("Admin@123");
                    passwordElement.SendKeys("Admin@123");

                    var loginButton = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                            By.CssSelector("button.btn.btn-primary")
                        )
                    );
                    loginButton.Click();
                }
                catch (WebDriverTimeoutException ex)
                catch (WebDriverTimeoutException ex)
                {
                    throw new Exception("Failed to find an element during login.", ex);
                }
            }
        }

        [Fact]
        public async Task TestAssingmentListDisplay()
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
                    emailElement.SendKeys("admin@example.com");

                    var passwordElement = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Input_Password")
                        )
                    );
                    passwordElement.SendKeys("Admin@123");

                    var loginButton = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                            By.CssSelector("button.btn.btn-primary")
                        )
                    );
                    loginButton.Click();
                }
                catch (WebDriverTimeoutException ex)
                {
                    throw new Exception("Failed to find an element during login.", ex);
                }
            }
            string errorLocationFunktion = "TestAssignmentListDiisplay: ";

            //test myAssignmentList
            _driver
                .Navigate()
                .GoToUrl(_baseUrl + "Assignment/ChangeTabel?currentList=MyAssignments");
            try
            {
                //test if AssignmentList is displayed
                IWebElement assignmentList = _wait.Until(d => d.FindElement(By.TagName("tbody")));
                try
                {
                    Assert.True(
                        assignmentList.Displayed,
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

                //test if there are no Assignments to show
                try
                {
                    assignmentList.FindElement(
                        By.XPath("//tr/td[text()='Keine Aufgaben vorhanden']")
                    );
                    throw new Exception(
                        ""
                            + _errorLocationClass
                            + errorLocationFunktion
                            + "AssignmentList-noAssignments-Check: noAssignmentsFound"
                    );
                }
                catch (NoSuchElementException e) { }

                //getTitles as Strings
                var titles = assignmentList.FindElements(By.XPath("//tr/td[1]"));
                List<String> TitleTexts = new List<String>();
                foreach (var t in titles)
                {
                    TitleTexts.Add(t.Text);
                }

                //check number of Titles
                try
                {
                    Assert.True(TitleTexts.Count() == 1);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        ""
                            + _errorLocationClass
                            + errorLocationFunktion
                            + "AssignmentList-numberOfTitles-Check: "
                            + e.Message
                    );
                }

                //check right titles are Presen
                try
                {
                    Assert.True(TitleTexts.Where(t => t == "TestUserUser").ToList().Count == 1);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        ""
                            + _errorLocationClass
                            + errorLocationFunktion
                            + "AssignmentList-TitlesText-Check: "
                            + e.Message
                    );
                }
                //test if assignments are correctly formed
                var assignments = assignmentList.FindElements(By.TagName("tr"));
                foreach (var a in assignments)
                {
                    string assignmentTitleText = "";
                    try
                    {
                        var assignmentTitle = a.FindElement(By.XPath("/td[1]"));
                        Assignment assignment = _context.Assignments.First(a =>
                            a.Title == assignmentTitle.Text
                        );
                        if (assignment == null)
                            throw new Exception(
                                ""
                                    + _errorLocationClass
                                    + errorLocationFunktion
                                    + "AssignmentList-AssignmentNotInDbButInList-Check: Assignment not in db"
                            );
                        assignmentTitleText = assignment.Title;
                        try
                        {
                            Assert.True("Test" == a.FindElement(By.XPath("/td[2]")).Text);
                        }
                        catch (Exception e)
                        {
                            throw new Exception(
                                ""
                                    + _errorLocationClass
                                    + errorLocationFunktion
                                    + "AssignmentList-Process-Check: "
                                    + e.Message
                            );
                        }
                        try
                        {
                            Assert.True(
                                assignment.GetDaysTillDueDate().ToString()
                                    == a.FindElement(By.XPath("/td[3]")).Text
                            );
                        }
                        catch (Exception e)
                        {
                            throw new Exception(
                                ""
                                    + _errorLocationClass
                                    + errorLocationFunktion
                                    + "AssignmentList-DaysTillDueDate-Check: "
                                    + e.Message
                            );
                        }
                        try
                        {
                            Assert.True(
                                assignment.DueDate.ToString("dd.MM.yyyy")
                                    == a.FindElement(By.XPath("/td[4]")).Text
                            );
                        }
                        catch (Exception e)
                        {
                            throw new Exception(
                                ""
                                    + _errorLocationClass
                                    + errorLocationFunktion
                                    + "AssignmentList-DueDate-Check: "
                                    + e.Message
                            );
                        }
                        try
                        {
                            Assert.True(
                                Helpers.EnumHelper.GetDisplayName(assignment.Status)
                                    == a.FindElement(By.XPath("/td[4]")).Text
                            );
                        }
                        catch (Exception e)
                        {
                            throw new Exception(
                                ""
                                    + _errorLocationClass
                                    + errorLocationFunktion
                                    + "AssignmentList-Status-Check: "
                                    + e.Message
                            );
                        }
                    }
                    catch (Xunit.Sdk.TrueException e)
                    {
                        throw new Exception(
                            "AssignmentList not Formed Correktly: MyAssignments: "
                                + assignmentTitleText
                                + "\n"
                                + e.Message
                        );
                    }
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

        private async Task ClearDbSet<T>()
            where T : class
        {
            var dbSet = _context.Set<T>();
            _context.RemoveRange(dbSet);
            await _context.SaveChangesAsync();
        }

        public async Task SeedDatabase()
        {
            //leere Datanbank
            await ClearDbSet<Assignment>();
            await ClearDbSet<Contract>();
            await ClearDbSet<Department>();
            await ClearDbSet<DueTime>();
            await ClearDbSet<Process>();

            //insert Required Roles:
            string[] roleNames = { "Administrator", "User", "Personal" };
            foreach (var roleName in roleNames)
            {
                ApplicationRole role = new ApplicationRole(roleName);
                if (await _roleManager.RoleExistsAsync(role.Name) == false)
                    await _roleManager.CreateAsync(role);
                _context.SaveChanges();
            }

            //insert Required Users
            ApplicationUser admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FullName = "Admin User"
            };
            if (_userManager.FindByEmailAsync(admin.Email) == null)
            {
                await _userManager.CreateAsync(admin, "Admin@123");
                await _userManager.AddToRoleAsync(admin, "Administrator");
            }

            ApplicationUser user = new ApplicationUser
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                FullName = "User User"
            };
            if (_userManager.FindByEmailAsync(user.Email) == null)
            {
                await _userManager.CreateAsync(user, "User@123");
                await _userManager.AddToRoleAsync(user, "User");
            }
            ApplicationUser personal = new ApplicationUser
            {
                UserName = "personal@example.com",
                Email = "personal@example.com",
                FullName = "Personal User"
            };
            if (_userManager.FindByEmailAsync(user.Email) == null)
            {
                await _userManager.CreateAsync(personal, "Personal@123");
                await _userManager.AddToRoleAsync(personal, "Personal");
            }
            //create Deparments and Contracts
            _context.Contracts.Add(new Contract("Test"));
            _context.Departments.Add(new Department("Test"));
            _context.SaveChanges();
            //create DueTime
            _context.DueTimes.AddRange(
                new DueTime("dueTime1", 1, 2, 3),
                new DueTime("dueTime2", 3, 1, 2)
            );
            //create Assignments
            DateTime DueDate = DateTime.Today;
            List<Assignment> assignmens =
                new()
                {
                    new Assignment(
                        new AssignmentTemplate
                        {
                            Title = "TestRolePersonal",
                            Instructions = "Test",
                            AssigneeType = Enums.AssigneeType.ROLES,
                            AssignedRole = await _roleManager.FindByNameAsync("Personal"),
                            DueIn = _context.DueTimes.Find(1),
                            ProcessTemplateId = 0
                        },
                        DueDate,
                        personal
                    ),
                    new Assignment(
                        new AssignmentTemplate
                        {
                            Title = "TestUserPersonal",
                            Instructions = "Test",
                            AssigneeType = Enums.AssigneeType.SUPERVISOR,
                            DueIn = _context.DueTimes.Find(2),
                            ProcessTemplateId = 0
                        },
                        DueDate,
                        personal
                    ),
                    new Assignment(
                        new AssignmentTemplate
                        {
                            Title = "TestUserUser",
                            Instructions = "Test",
                            AssigneeType = Enums.AssigneeType.WORKER_OF_REF,
                            DueIn = _context.DueTimes.Find(1),
                            ProcessTemplateId = 0
                        },
                        DueDate,
                        user
                    ),
                    new Assignment(
                        new AssignmentTemplate
                        {
                            Title = "TestUserUser",
                            Instructions = "Test",
                            AssigneeType = Enums.AssigneeType.SUPERVISOR,
                            DueIn = _context.DueTimes.Find(1),
                            ProcessTemplateId = 0
                        },
                        DueDate,
                        personal
                    )
                };
            _context.Assignments.AddRange(assignmens);
            _context.SaveChanges();

            List<Process> processes = new List<Process>
            {
                new Process(
                    "Test",
                    "Test",
                    _context.Assignments.ToList(),
                    user,
                    personal,
                    _context.Contracts.First(),
                    _context.Departments.First()
                )
            };
            _context.Processes.AddRange(processes);
            _context.SaveChanges();
        }
    }
} //end code ownership Jan Pfluger

