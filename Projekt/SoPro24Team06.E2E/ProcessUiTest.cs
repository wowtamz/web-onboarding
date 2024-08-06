//-------------------------
// Author: Tamas Varadi
//-------------------------

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
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using Xunit;

namespace SoPro24Team06.E2E;

[Collection("Sequential")]
public class ProcessUiTest :
    IClassFixture<CustomWebApplicationFactory<Program>>,
    IDisposable
{
    private readonly string baseurl = "https://localhost:7003/";
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _webClient;


    public ProcessUiTest()
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
            emailElement.SendKeys("personal@example.com");
            var passwordElement = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                    By.Id("Input_Password")
                )
            );
            passwordElement.SendKeys("Personal@123");
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


    [Fact]
    public async Task StartProcessFromProcessTemplate()
    {

        Login();

        _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.TitleContains("- SoPro24Team06"));
        
        AssertUrlContains("Assignment");

        IWebElement processTemplateLink = FindWebElementByTagAndValue("a", "Prozesse");
        processTemplateLink.Click();
        
        AssertUrlContains("ProcessTemplate");
        AssertTitleContains("Prozesse");
        
        IWebElement linkElement = FindWebElementByXPath("//a[contains(@href, '/Process/Start/')]");
        linkElement.Click();
        
        AssertUrlContains("Process/Start/");
        AssertTitleContains("Vorgang starten");

        string processTitle = "";
        IWebElement inputTitle = FindWebElementById("Title");
        processTitle = inputTitle.GetAttribute("value");

        IWebElement dropdownWorkerOfRef = FindWebElementById("workerOfRefDropdown");
        SelectElement selectWorkerOfRef = new SelectElement(dropdownWorkerOfRef);
        selectWorkerOfRef.SelectByIndex(1);
        
        IWebElement dropdownContract = FindWebElementById("contractDropdown");
        SelectElement selectContract = new SelectElement(dropdownContract);
        selectContract.SelectByIndex(1);
        
        IWebElement dropdownDepartment = FindWebElementById("departmentDropdown");
        SelectElement selectDepartment = new SelectElement(dropdownDepartment);
        selectDepartment.SelectByIndex(1);

        IWebElement startButton = FindWebElementByTagAndValue("button", "Starten");
        startButton.Click();
            
        AssertTitleContains("Vorgänge");

        IWebElement tdElement = FindWebElementByXPath($"//td[text()='{processTitle}']");

        Assert.NotNull(tdElement);
    }
    
    [Fact]
    public void StartProcessFromProcessPage()
    {

        Login();

        _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.TitleContains("- SoPro24Team06"));

        IWebElement processLinkElement = FindWebElementByTagAndValue("a", "Vorgänge");
        processLinkElement.Click();
        
        AssertUrlContains("Process");
        AssertTitleContains("Vorgänge");
        
        IWebElement linkElement = FindWebElementByXPath("//a[contains(@href, '/Process/Start')]");
        linkElement.Click();
        
        AssertUrlContains("Process/Start");
        AssertTitleContains("Vorgang starten");

        IWebElement dropdownTemplate = FindWebElementById("processTemplateDropdown");
        SelectElement selectTemplate = new SelectElement(dropdownTemplate);
        selectTemplate.SelectByIndex(1);

        string processTitle = "Test process title";
        IWebElement inputTitle = FindWebElementById("Title");
        inputTitle.Clear();
        inputTitle.SendKeys(processTitle);

        IWebElement dropdownWorkerOfRef = FindWebElementById("workerOfRefDropdown");
        SelectElement selectWorkerOfRef = new SelectElement(dropdownWorkerOfRef);
        selectWorkerOfRef.SelectByIndex(1);
        
        IWebElement dropdownContract = FindWebElementById("contractDropdown");
        SelectElement selectContract = new SelectElement(dropdownContract);
        selectContract.SelectByIndex(1);

        
        IWebElement dropdownDepartment = FindWebElementById("departmentDropdown");
        SelectElement selectDepartment = new SelectElement(dropdownDepartment);
        selectDepartment.SelectByIndex(1);

        IWebElement startButton = FindWebElementByTagAndValue("button", "Starten");
        startButton.Click();
        
        AssertTitleContains("Vorgänge");
        
        IWebElement tdElement = FindWebElementByXPath($"//td[text()='{processTitle}']");
        Assert.NotNull(tdElement);
    }
    
    
    [Fact]
    public void StartProcessFromTemplateAddCustomAssignment()
    {

        Login();

        _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.TitleContains("- SoPro24Team06"));
        
        //Assignments Page
        AssertUrlContains("Assignment");

        IWebElement processTemplateLink = FindWebElementByTagAndValue("a", "Prozesse");
        processTemplateLink.Click();
        
        
        //ProcessTemplates Page
        AssertUrlContains("ProcessTemplate");
        AssertTitleContains("Prozesse");
        
        IWebElement linkElement = FindWebElementByXPath("//a[contains(@href, '/Process/Start/')]");
        linkElement.Click();
        
        //Start Process Page
        AssertUrlContains("Process/Start/");
        AssertTitleContains("Vorgang starten");
        
        IWebElement createAssignmentTemplateButton = FindWebElementById("createAssignmentTemplateButton");
        createAssignmentTemplateButton.Click();
        
        //Create AssignmentTemplate Page
        AssertUrlContains("AssignmentTemplate/Create");

        string assignmentTemplateTitle = "New Assignment Template";
        IWebElement inputAssignmentTitle = FindWebElementById("Title");
        inputAssignmentTitle.SendKeys(assignmentTemplateTitle);
        
        // Select AssigneeType: Role
        string assigneeType = "Rolle";
        IWebElement dropdownAssigneeType = FindWebElementById("assigneeType");
        SelectElement selectAssigneeType = new SelectElement(dropdownAssigneeType);
        selectAssigneeType.SelectByText(assigneeType);
        
        // Select AssignedRole: First option
        IWebElement dropdownAssignedRole = FindWebElementById("AssignedRole");
        SelectElement selectAssignedRole = new SelectElement(dropdownAssignedRole);
        selectAssignedRole.SelectByIndex(1);
        string selectedRole = selectAssignedRole.SelectedOption.Text;
        
        // Select DueTime: First option
        IWebElement dropdownDueTime = FindWebElementById("dueIn");
        SelectElement selectDueTime = new SelectElement(dropdownDueTime);
        selectDueTime.SelectByText("ASAP");
        string selectedDueTime = selectDueTime.SelectedOption.Text;
        
        IWebElement createButton = FindWebElementByTagAndValue("button", "Erstellen");
        createButton.Click();
        
        //Start Process Page
        AssertUrlContains("Process/Start");
        AssertTitleContains("Vorgang starten");

        IWebElement inputTitle = FindWebElementById("Title");
        string processTitle = inputTitle.GetAttribute("value");
        
        IWebElement inputDescription = FindWebElementById("Description");
        string processDescription = inputDescription.GetAttribute("value");

        IWebElement dropdownWorkerOfRef = FindWebElementById("workerOfRefDropdown");
        SelectElement selectWorkerOfRef = new SelectElement(dropdownWorkerOfRef);
        selectWorkerOfRef.SelectByIndex(1);
        string workerOfRef = selectWorkerOfRef.SelectedOption.Text;
        
        IWebElement dropdownContract = FindWebElementById("contractDropdown");
        SelectElement selectContract = new SelectElement(dropdownContract);
        selectContract.SelectByIndex(1);
        string contract = selectWorkerOfRef.SelectedOption.Text;
        
        IWebElement dropdownDepartment = FindWebElementById("departmentDropdown");
        SelectElement selectDepartment = new SelectElement(dropdownDepartment);
        selectDepartment.SelectByIndex(1);
        string department = selectWorkerOfRef.SelectedOption.Text;
        
        IWebElement assignmentInput =
            FindWebElementByXPath(
                $"//tr[td[1][text() = '{assignmentTemplateTitle}'] and td[2][text() = '{assigneeType}'] and input[@value = '{selectedDueTime}' and @name = 'AssignmentTemplates[1].DueIn.Label']]\n");
        IWebElement startButton = FindWebElementByTagAndValue("button", "Starten");
        startButton.Click();
            
        AssertTitleContains("Vorgänge");
        IWebElement tdElement = FindWebElementByXPath($"//tr[td[text()='{processTitle}']]");

        Assert.NotNull(tdElement);
    }

    public void AssertUrlContains(string url)
    {
        if (!_driver.Url.Contains(url))
        {
            throw new Exception($"Page url does not contain: {url}");
        }
    }

    public void AssertTitleContains(string title)
    {
        if (!_driver.Title.Contains(title))
        {
            throw new Exception($"Page title does not contain: {title}");
        }
    }

    public IWebElement FindWebElementById(string id)
    {
        IWebElement webElement = null;
        try
        {
             webElement = _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                    By.Id(id)
                )
            );
        }
        catch (NoSuchElementException)
        {
            throw new Exception($"No element was found with id: {id}");
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new Exception($"WedDriver timedout during wait for element with id {id}: " + exception.Message);
        }
        
        return webElement;
    }

    public IWebElement FindWebElementByTagAndValue(string tag, string value)
    {
        IWebElement webElement = null;
        try
        {
            webElement = _wait.Until(driver =>
            {
                var buttons = driver.FindElements(By.TagName(tag));
                IWebElement button = buttons.FirstOrDefault(b =>
                    b.Text.Equals(value, StringComparison.OrdinalIgnoreCase)
                );

                return button;
            });
        }
        catch (NoSuchElementException)
        {
            throw new Exception($"No element was found with tag ({tag}) and value ({value})");
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new Exception($"WedDriver timedout during wait for element tag ({tag}) and value ({value}): " + exception.Message);
        }

        return webElement;
    }

    public IWebElement FindWebElementByXPath(string expression)
    {
        IWebElement webElement = null;
        try
        {
            webElement = _wait.Until(driver =>
            {
                IWebElement element = driver.FindElement(By.XPath(expression));
                return element.Displayed ? element : null;
            });
        }
        catch (NoSuchElementException)
        {
            throw new Exception($"No element was found with xpath: {expression})");
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new Exception($"WedDriver timedout during wait for element with xpath ({expression}): " + exception.Message);
        }

        return webElement;
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
        context.DueTimes.Add(new DueTime("ASAP", 1, 0, 0));
        context.DueTimes.Add(new DueTime("2 Wochen vor Start", 0, -2, 0));
        context.DueTimes.Add(new DueTime("3 Wochen nach Arbeitsbeginn", 0, 3, 0));
        context.SaveChanges();
        
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

        List<AssignmentTemplate> assignmentTemplates =
            new()
            {
                new AssignmentTemplate
                {
                    AssignedRole = context.Roles.First(),
                    AssigneeType = AssigneeType.ROLES,
                    DueIn = context.DueTimes.FirstOrDefault(),
                    ForContractsList = new List<Contract> { context.Contracts.First() },
                    ForDepartmentsList = new List<Department> { context.Departments.First() },
                    Instructions = "Instructions",
                    Title = "assignment template"
                }
            };

        await context.AssignmentTemplates.AddRangeAsync(assignmentTemplates);
        await context.SaveChangesAsync();

        List<ProcessTemplate> processTemplates = new List<ProcessTemplate>
        {
            new ProcessTemplate
            {
                AssignmentTemplates = new List<AssignmentTemplate> { context.AssignmentTemplates.First() },
                ContractOfRefWorker = context.Contracts.First(),
                DepartmentOfRefWorker = context.Departments.First(),
                Description = "Description",
                RolesWithAccess = context.Roles.Where(r => r.Name == "Personal").ToList(),
                Title = "Prozess Title"
            }
        };

        await context.ProcessTemplates.AddRangeAsync(processTemplates);
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
