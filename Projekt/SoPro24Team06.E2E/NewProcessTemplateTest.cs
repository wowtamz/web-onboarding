//-------------------------
// Author: Michael Adolf
// Grundstruktur (Konstruktor, Login(), Dispose, ClearDBSet) von Jan Pfluger
// Test ist noch nicht funktionsfähig, einige Details fehlen!
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
using SeleniumExtras;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using Xunit;

namespace SoPro24Team06.E2E
{
    public class ProcessUiTest : IDisposable
    {
        private readonly string _errorLocationClass = "ProcessUiTest";
        private readonly string _baseUrl = "https://localhost:7003/";
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ProcessUiTest()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-dev-shm-usage");
            chromeOptions.AddArguments("--no-sandbox");
            chromeOptions.AddArguments("--window-size=1920,1080");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            _driver = new ChromeDriver(chromeDriverService, chromeOptions);

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));

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
                _context = context;
                context.Database.EnsureCreated();

                _userManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<ApplicationUser>
                >();
                _roleManager = scope.ServiceProvider.GetRequiredService<
                    RoleManager<ApplicationRole>
                >();
            }
        }

        public void Login()
        {
            _driver.Navigate().GoToUrl(_baseUrl);

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
        }

        [Fact]
        public void TestCreateProcessWithMultipleAssignments()
        {
            Login();
            string errorLocationFunktion = "TestCreateProcessWithMultipleAssignments: ";

            // Navigate to the process creation page
            _driver.Navigate().GoToUrl(_baseUrl + "Process/Create");
            try
            {
                // Fill out process details
                var processNameElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("ProcessName")
                    )
                );
                processNameElement.SendKeys("New Process");

                var processDescriptionElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("ProcessDescription")
                    )
                );
                processDescriptionElement.SendKeys("This is a new process with multiple tasks.");
                var addTaskButton = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("AddTaskButton")
                    )
                );
                addTaskButton.Click();

                var taskTitleElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Name("TaskTitle")
                    )
                );
                taskTitleElement.SendKeys("Task 1");

                var taskDueDateElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Name("TaskDueDate")
                    )
                );
                taskDueDateElement.SendKeys(DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"));

                var saveButton = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("button[type='submit']")
                    )
                );
                saveButton.Click();

                var successMessage = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.CssSelector(".alert-success")
                    )
                );
                Assert.True(successMessage.Displayed, "Process was not created successfully.");
            }
            catch (NoSuchElementException e)
            {
                throw new Exception(
                    "Error: Create Process UI elements not found correctly: \n" + e.Message
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
            // Clear and seed database similar to the existing method
            await ClearDbSet<Assignment>();
            await ClearDbSet<Contract>();
            await ClearDbSet<Department>();
            await ClearDbSet<DueTime>();
            await ClearDbSet<Process>();

            // Insert required roles and users
            string[] roleNames = { "Administrator", "User", "Personal" };
            foreach (var roleName in roleNames)
            {
                ApplicationRole role = new ApplicationRole(roleName);
                if (await _roleManager.RoleExistsAsync(role.Name) == false)
                    await _roleManager.CreateAsync(role);
                _context.SaveChanges();
            }

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

            ApplicationUser personal = new ApplicationUser
            {
                UserName = "personal@example.com",
                Email = "personal@example.com",
                FullName = "Personal User"
            };
            if (_userManager.FindByEmailAsync(personal.Email) == null)
            {
                await _userManager.CreateAsync(personal, "Personal@123");
                await _userManager.AddToRoleAsync(personal, "Personal");
            }
        }
    }
}
