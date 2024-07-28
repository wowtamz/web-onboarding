//-------------------------
// Author: Jan Pfluger 
//-------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using Xunit;

namespace SoPro24Team06.E2E
{
    public class AssignmentUiTestChangeAssignmentStatus : IClassFixture<TestFixtures>
    {
        private readonly TestFixtures _fixture;
        private readonly ChromeDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly string _errorLocationClass = "AssignmentUiTestChangeAssignmentStatus: ";

        public AssignmentUiTestChangeAssignmentStatus(TestFixtures fixture)
        {
            _fixture = fixture;
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-dev-shm-usage");
            chromeOptions.AddArguments("--no-sandbox");
            chromeOptions.AddArguments("--window-size=1920,1080");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            _driver = new ChromeDriver(chromeDriverService, chromeOptions);

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
        }

        private void WaitForElement(By by, int timeoutInSeconds = 60)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
        }

        public void Login(string email, string password)
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
                    emailElement.SendKeys(email);

                    var passwordElement = _wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                            By.Id("Input_Password")
                        )
                    );
                    passwordElement.SendKeys(password);

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
        }
        //-------------------------
        // Author: Vincent Steiner
        //-------------------------
        [Fact]
        public void TestAssignmentStatus() // Versuch eines E2E Tests bin aber gescheitert, wie das restliche Team
        {
            try
            {

                _driver.Navigate().GoToUrl("https://localhost:7003/");
                Login("user@example.com", "User@123");
                _driver.Navigate().GoToUrl(_fixture.BaseUrl + "Assignment");

                WaitForElement(By.TagName("tbody")); 

                var assignmentList = _driver.FindElement(By.TagName("tbody"));
                var rows = assignmentList.FindElements(By.TagName("tr"));

                if (rows.Count == 0)
                {
                    throw new Exception("No assignments available in the table.");
                }

                
                var firstRow = rows[0];
                var editButton = firstRow.FindElement(By.XPath(".//a[./i[contains(@class, 'fa-edit')]]"));
                editButton.Click();

                
                WaitForElement(By.XPath("//h1[text()='Edit Assignment']"));

                
                var statusDropdown = _wait.Until(d => d.FindElement(By.Id("assignmentStatusDropdown")));
                var selectElement = new SelectElement(statusDropdown);
                selectElement.SelectByText("IN_PROGRESS");

                var saveButton = _wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));
                saveButton.Click();

                
                WaitForElement(By.TagName("tbody")); 
                var updatedRows = _driver.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                var updatedFirstRow = updatedRows[0];
                var statusCell = updatedFirstRow.FindElement(By.XPath(".//td[position()=5]"));

                var statusText = statusCell.Text;
                if (statusText != "IN_PROGRESS")
                {
                    throw new Exception($"Expected status 'IN_PROGRESS', but found '{statusText}'.");
                }

                editButton.Click();
                statusDropdown = _wait.Until(d => d.FindElement(By.Id("assignmentStatusDropdown")));
                selectElement = new SelectElement(statusDropdown);
                selectElement.SelectByText("DONE");

                saveButton = _wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));
                saveButton.Click();

                WaitForElement(By.TagName("tbody"));
                var finalRows = _driver.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                var finalFirstRow = finalRows[0];
                var finalStatusCell = finalFirstRow.FindElement(By.XPath(".//td[position()=5]"));

                var finalStatusText = finalStatusCell.Text;
                if (finalStatusText != "DONE")
                {
                    throw new Exception($"Expected status 'DONE', but found '{finalStatusText}'.");
                }
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new Exception("Timeout occurred during the test execution.", ex);
            }
            catch (NoSuchElementException ex)
            {
                throw new Exception("Element not found during the test execution.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during the test execution.", ex);
            }
        }

        private async Task ClearDbSet<T>()
            where T : class
        {
            var dbSet = _fixture.Context.Set<T>();
            _fixture.Context.RemoveRange(dbSet);
            await _fixture.Context.SaveChangesAsync();
        }

        public async Task SeedDatabase()
        {
            await ClearDbSet<Assignment>();
            await ClearDbSet<Contract>();
            await ClearDbSet<Department>();
            await ClearDbSet<DueTime>();
            await ClearDbSet<Process>();

            string[] roleNames = { "Administrator", "User", "Personal" };
            foreach (var roleName in roleNames)
            {
                ApplicationRole role = new ApplicationRole(roleName);
                if (!await _fixture.RoleManager.RoleExistsAsync(role.Name))
                    await _fixture.RoleManager.CreateAsync(role);
                _fixture.Context.SaveChanges();
            }


            ApplicationUser admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FullName = "Admin User"
            };
            if (await _fixture.UserManager.FindByEmailAsync(admin.Email) == null)
            {
                await _fixture.UserManager.CreateAsync(admin, "Admin@123");
                await _fixture.UserManager.AddToRoleAsync(admin, "Administrator");
            }

            ApplicationUser user = new ApplicationUser
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                FullName = "User User"
            };
            if (await _fixture.UserManager.FindByEmailAsync(user.Email) == null)
            {
                await _fixture.UserManager.CreateAsync(user, "User@123");
                await _fixture.UserManager.AddToRoleAsync(user, "User");
            }

            ApplicationUser personal = new ApplicationUser
            {
                UserName = "personal@example.com",
                Email = "personal@example.com",
                FullName = "Personal User"
            };
            if (await _fixture.UserManager.FindByEmailAsync(personal.Email) == null)
            {
                await _fixture.UserManager.CreateAsync(personal, "Personal@123");
                await _fixture.UserManager.AddToRoleAsync(personal, "Personal");
            }

            _fixture.Context.Contracts.Add(new Contract("Test"));
            _fixture.Context.Departments.Add(new Department("Test"));
            _fixture.Context.SaveChanges();

            _fixture.Context.DueTimes.AddRange(
                new DueTime("dueTime1", 1, 2, 3),
                new DueTime("dueTime2", 3, 1, 2)
            );

            DateTime dueDate = DateTime.Today;
            List<Assignment> assignments = new()
            {
                new Assignment(
                    new AssignmentTemplate
                    {
                        Title = "TestRolePersonal",
                        Instructions = "Test",
                        AssigneeType = Enums.AssigneeType.ROLES,
                        AssignedRole = await _fixture.RoleManager.FindByNameAsync("Personal"),
                        DueIn = _fixture.Context.DueTimes.Find(1),
                        ProcessTemplateId = 0
                    },
                    dueDate,
                    personal
                ),
                new Assignment(
                    new AssignmentTemplate
                    {
                        Title = "TestUserPersonal",
                        Instructions = "Test",
                        AssigneeType = Enums.AssigneeType.SUPERVISOR,
                        DueIn = _fixture.Context.DueTimes.Find(2),
                        ProcessTemplateId = 0
                    },
                    dueDate,
                    personal
                ),
                new Assignment(
                    new AssignmentTemplate
                    {
                        Title = "TestUserUser",
                        Instructions = "Test",
                        AssigneeType = Enums.AssigneeType.WORKER_OF_REF,
                        DueIn = _fixture.Context.DueTimes.Find(1),
                        ProcessTemplateId = 0
                    },
                    dueDate,
                    user
                ),
                new Assignment(
                    new AssignmentTemplate
                    {
                        Title = "TestUserUser",
                        Instructions = "Test",
                        AssigneeType = Enums.AssigneeType.SUPERVISOR,
                        DueIn = _fixture.Context.DueTimes.Find(1),
                        ProcessTemplateId = 0
                    },
                    dueDate,
                    personal
                )
            };
            _fixture.Context.Assignments.AddRange(assignments);
            _fixture.Context.SaveChanges();

            List<Process> processes = new List<Process>
            {
                new Process(
                    "Test",
                    "Test",
                    _fixture.Context.Assignments.ToList(),
                    user,
                    personal,
                    _fixture.Context.Contracts.First(),
                    _fixture.Context.Departments.First()
                )
            };
            _fixture.Context.Processes.AddRange(processes);
            _fixture.Context.SaveChanges();
        }
    }
}

