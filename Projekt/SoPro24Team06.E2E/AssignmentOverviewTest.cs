using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SoPro24Team06.Controllers;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using SoPro24Team06.Enums;
using Xunit;

namespace SoPro24Team06.E2E
{
    public class AssignmentOverviewTest : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
    {
        // Class-level variables
        private readonly string baseurl = "https://localhost:7003/";
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _webClient;

        // Constructor
        public AssignmentOverviewTest()
        {
            // Set environment variable for testing
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            // Initialize custom web application factory
            _factory = new CustomWebApplicationFactory<Program>();
            _webClient = _factory.CreateDefaultClient();

            // Setup data for testing
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                SetTestData(context, userManager, roleManager).Wait();
            }

            // Selenium ChromeDriver setup
            Environment.SetEnvironmentVariable("DISPLAY", ":99");
            var options = new ChromeOptions();
            options.AddArguments("--no-sandbox", "--headless", "--disable-gpu", "--disable-dev-shm-usage", "--disable-extensions", "--disable-infobars", "--remote-debugging-port=9222", "--window-size=2560,1440", "enable-automation", "--disable-browser-side-navigation", "--ignore-certificate-errors");

            var service = ChromeDriverService.CreateDefaultService();
            service.LogPath = "chromedriver.log";
            service.EnableVerboseLogging = true;

            _driver = new ChromeDriver(service, options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
        }

        // Dispose method to clean up resources
        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
            _webClient.Dispose();
            _factory.Dispose();
        }

        // Data setup method
        public async Task SetTestData(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager
        )
        {
            // Clear and recreate the database
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Insert required roles
            string[] roleNames = { "Administrator", "User", "Personal" };
            foreach (var roleName in roleNames)
            {
                ApplicationRole role = new ApplicationRole(roleName);
                if (!await roleManager.RoleExistsAsync(role.Name))
                    await roleManager.CreateAsync(role);
                await context.SaveChangesAsync();
            }

            // Insert required users
            var users = new[]
            {
                new ApplicationUser { UserName = "admin@example.com", Email = "admin@example.com", FullName = "Admin User" },
                new ApplicationUser { UserName = "user@example.com", Email = "user@example.com", FullName = "User User" },
                new ApplicationUser { UserName = "personal@example.com", Email = "personal@example.com", FullName = "Personal User" }
            };

            foreach (var user in users)
            {
                if (await userManager.FindByEmailAsync(user.Email) == null)
                {
                    await userManager.CreateAsync(user, "User@123");
                    await userManager.AddToRoleAsync(user, user.UserName.StartsWith("admin") ? "Administrator" : user.UserName.StartsWith("personal") ? "Personal" : "User");
                }
            }

            // Add assignments and processes
            var assignments1 = new List<Assignment>
            {
                new Assignment { Title = "Assignment 1.1", Instructions = "Instructions 1.1", Status = AssignmentStatus.NOT_STARTED, DueDate = DateTime.Now.AddDays(5) },
                new Assignment { Title = "Assignment 1.2", Instructions = "Instructions 1.2", Status = AssignmentStatus.NOT_STARTED, DueDate = DateTime.Now.AddDays(10) }
            };

            var assignments2 = new List<Assignment>
            {
                new Assignment { Title = "Assignment 2.1", Instructions = "Instructions 2.1", Status = AssignmentStatus.NOT_STARTED, DueDate = DateTime.Now.AddDays(15) },
                new Assignment { Title = "Assignment 2.2", Instructions = "Instructions 2.2", Status = AssignmentStatus.NOT_STARTED, DueDate = DateTime.Now.AddDays(20) }
            };

            var process1 = new Process
            {
                Title = "Vorgang 1",
                Description = "Description 1",
                Assignments = assignments1
            };

            var process2 = new Process
            {
                Title = "Vorgang 2",
                Description = "Description 2",
                Assignments = assignments2
            };

            await context.Processes.AddRangeAsync(process1, process2);
            await context.SaveChangesAsync();
        }

        // Test method to verify processes in overview
        [Fact]
        public void Verify_Processes_In_Overview()
        {
            // Login to the application
            _driver.Navigate().GoToUrl("https://localhost:7003/");

            var emailElement = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("Input_Email")));
            emailElement.SendKeys("user@example.com");
            var passwordElement = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("Input_Password")));
            passwordElement.SendKeys("User@123");
            var loginButton = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector("[aria-label='login-submit']")));
            loginButton.Click();

            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.TitleContains("- SoPro24Team06"));

            // Navigate to the assignment overview page
            _driver.Navigate().GoToUrl("https://localhost:7003/Assignment/ChangeTable?currentList=AllAssignments");

            // Verify that the table for all assignments is present
            var allAssignmentsTable = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("allAssignmentsTable")));
            var rows = allAssignmentsTable.FindElements(By.CssSelector("tbody tr"));

            // Verify the first process and its assignments
            Assert.Contains("Vorgang 1", rows[0].Text);
            Assert.Contains("Assignment 1.1", rows[0].Text);
            Assert.Contains("Assignment 1.2", rows[1].Text);

            // Verify the second process and its assignments
            Assert.Contains("Vorgang 2", rows[2].Text);
            Assert.Contains("Assignment 2.1", rows[2].Text);
            Assert.Contains("Assignment 2.2", rows[3].Text);
        }
    }
}
