using System;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SoPro24Team06.Controllers;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using Xunit;

namespace SoPro24Team06.E2E;

[Collection("Sequential")]
public class AssignmentUiTestChangeAssignmentStatus
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IDisposable
{
    private readonly string baseurl = "https://localhost:7003/";
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _webClient;

    public AssignmentUiTestChangeAssignmentStatus()
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

            SetTestData(context, userManager, roleManager).Wait();
        }

        Environment.SetEnvironmentVariable("DISPLAY", ":99");

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
        service.LogPath = "chromedriver.log";
        service.EnableVerboseLogging = true;

        _driver = new ChromeDriver(service, options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
        _webClient.Dispose();
        _factory.Dispose();
    }

    public void Login()
    {
        _driver.Navigate().GoToUrl("https://localhost:7003/");

        try
        {
            var emailElement = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("Input_Email"))
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
        catch (WebDriverTimeoutException ex)
        {
            throw new Exception("Failed to find an element during login.", ex);
        }
    }

    //-------------------------
    // Author: Vincent Steiner
    //-------------------------
    [Fact]
    public void New_Employee_Change_AssignmentStatus()
    {
        // Login();
        _driver.Navigate().GoToUrl("https://localhost:7003/");

        var emailElement = _wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("Input_Email"))
        );
        emailElement.SendKeys("user@example.com");
        var passwordElement = _wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("Input_Password"))
        );
        passwordElement.SendKeys("User@123");
        var loginButton = _wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                By.CssSelector("[aria-label='login-submit']")
            )
        );
        loginButton.Click();

        _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.TitleContains("- SoPro24Team06"));

        _driver
            .Navigate()
            .GoToUrl("https://localhost:7003/Assignment/ChangeTable?currentList=AllAssignments");

        try
        {
            var assignmentsTable = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                    By.Id("allAssignmentsBody")
                )
            );
            var rows = assignmentsTable.FindElements(By.CssSelector("tbody tr"));

            var editButton = rows[0].FindElement(By.CssSelector("[id^='Edit-']")); // Hier sollte der korrekte Selektor verwendet werden
            editButton.Click();

            var dropdownElement = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                    By.Id("assignmentStatusDropdown")
                )
            );
            var selectElement = new SelectElement(dropdownElement);

            selectElement.SelectByText("In Bearbeitung");

            var submitButton = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                    By.Id("submitChanges")
                )
            );
            submitButton.Click();

            var statusElement = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("status"))
            );
            Assert.Equal("In Bearbeitung", statusElement.Text);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var assignmentsList = context.Assignments.ToList();
                var assignment = assignmentsList.FirstOrDefault(at => at.Title == "TestUserUser");
                Assert.Equal("IN_PROGRESS", assignment.Status.ToString());
            }

            _driver
                .Navigate()
                .GoToUrl(
                    "https://localhost:7003/Assignment/ChangeTable?currentList=AllAssignments"
                );

            assignmentsTable = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                    By.Id("allAssignmentsBody")
                )
            );
            rows = assignmentsTable.FindElements(By.CssSelector("tbody tr"));

            editButton = rows[0].FindElement(By.CssSelector("[id^='Edit-']"));
            editButton.Click();

            dropdownElement = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                    By.Id("assignmentStatusDropdown")
                )
            );
            selectElement = new SelectElement(dropdownElement);

            selectElement.SelectByText("Fertig");

            submitButton = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                    By.Id("submitChanges")
                )
            );
            submitButton.Click();

            statusElement = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("status"))
            );
            Assert.Equal("Fertig", statusElement.Text);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var assignmentsList = context.Assignments.ToList();
                var assignment = assignmentsList.FirstOrDefault(at => at.Title == "TestUserUser");
                Assert.Equal("DONE", assignment.Status.ToString());
            }
        }
        catch (NoSuchElementException ex)
        {
            Dispose();
            throw new Exception("Failed to find an element during test execution.", ex);
        }
        finally
        {
            Dispose();
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
        if (await userManager.FindByEmailAsync(personal.Email) == null)
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
}
