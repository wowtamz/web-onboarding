using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Containers;
using SoPro24Team06.Controllers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using SoPro24Team06.Interfaces;

namespace SoPro24Team06.E2E
{
    /// <summary>
    /// Frontendtest for AssignmentList
    /// </summary>
    //[Collection("Sequential")]
 
    public class AssignmentStatusUiTest : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly ApplicationDbContext _context;
        private readonly List<ApplicationUser> _users;
        private readonly List<ApplicationRole> _roles;
        private readonly List<IdentityUserRole<string>> _userRoles;

        private const string URI = "https://localhost:7003/";
        /// <summary>
        /// Creates and initializes chrome driver.
        /// </summary>
        /// <owner>Nicolai Kirndorfer</owner>
        public AssignmentStatusUiTest() 
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<ApplicationRole>>();
            _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(roleStore.Object, null, null, null, null);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            //SeedData();

            //Initialize(_mockUserManager, _mockRoleManager, _context);

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(
                "--window-size=2560,1440",
                "enable-automation",
                "--headless",
                "--no-sandbox",
                "--disable-infobars",
                "--disable-dev-shm-usage",
                "--disable-browser-side-navigation",
                "--disable-gpu",
                "--ignore-certificate-errors");
            _driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), chromeOptions, TimeSpan.FromMinutes(3));
            _driver.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(30));
        }

        private void SeedData()
        {
            // Roles
            var roleNames = new[] { "Administrator", "IT", "Backoffice", "Geschäftsleitung", "Personal" };
            foreach (var roleName in roleNames)
            {
                if (!_context.Roles.Any(r => r.Name == roleName))
                {
                    _context.Roles.Add(new ApplicationRole { Name = roleName });
                }
            }
            _context.SaveChanges();

            // Users
            var users = new List<(string UserName, string FullName, string Email, string Password, string[] Roles)>
            {
                ("admin@gmx.de", "Admin", "admin@gmx.de", "Admin1!", new[] { "Administrator" }),
                ("koenig@gmx.de", "König v. Augsburg", "koenig@gmx.de", "GHLutz13!", new[] { "Geschäftsleitung" }),
                ("gerhard@gmx.de", "Gerhard", "gerhard@gmx.de", "Gerhard1!", new[] { "Backoffice" }),
                ("bill@gmx.de", "Bill Yard", "bill@gmx.de", "B1llard!", new[] { "Personal", "Backoffice" }),
                ("karl@gmx.de", "Karl Toffel", "karl@gmx.de", "Erdapfel1!", new[] { "IT" }),
                ("wilma@gmx.de", "Wilma Ruhe", "wilma@gmx.de", "Pssssst1!", new string[] { })
            };

            foreach (var (userName, fullName, email, password, roles) in users)
            {
                if (!_context.Users.Any(u => u.UserName == userName))
                {
                    var user = new ApplicationUser { UserName = userName, FullName = fullName, Email = email };
                    var result = _mockUserManager.Object.CreateAsync(user, password).Result;
                    if (result.Succeeded)
                    {
                        _mockUserManager.Object.AddToRolesAsync(user, roles).Wait();
                    }
                }
            }
            _context.SaveChanges();
/*
            // Contracts
            var contracts = new[] { "Festanstellung", "Trainee", "Praktikant", "Werkstudent" };
            foreach (var label in contracts)
            {
                if (!_context.Contracts.Any(c => c.Label == label))
                {
                    _context.Contracts.Add(new Contract { Label = label });
                }
            }
            _context.SaveChanges();

            // Departments
            var departments = new[] { "Entwicklung", "Operations", "UI/UX", "Projektmanagement", "Backoffice", "Sales", "People & Culture" };
            foreach (var name in departments)
            {
                if (!_context.Departments.Any(d => d.Name == name))
                {
                    _context.Departments.Add(new Department { Name = name });
                }
            }
            _context.SaveChanges();

            // DueTimes
            var dueTimes = new[]
            {
                new DueTime { Label = "ASAP", Days = 0, Weeks = 0, Months = 0 },
                new DueTime { Label = "2 Wochen vor Start", Days = -14 },
                new DueTime { Label = "3 Wochen nach Arbeitsbeginn", Days = 21 },
                new DueTime { Label = "3 Monate nach Arbeitsbeginn", Months = 3 },
                new DueTime { Label = "6 Monate nach Arbeitsbeginn", Months = 6 }
            };
            foreach (var dueTime in dueTimes)
            {
                if (!_context.DueTimes.Any(dt => dt.Label == dueTime.Label))
                {
                    _context.DueTimes.Add(dueTime);
                }
            }
            _context.SaveChanges();

            // ProcessTemplates
            if (!_context.ProcessTemplates.Any(pt => pt.Title == "Onboarding"))
            {
                var processTemplate = new ProcessTemplate
                {
                    Title = "Onboarding",
                    Description = "Onboarding neuer Mitarbeiter",
                    RolesWithAccess = new List<ApplicationRole> { _context.Roles.First(r => r.Name == "Administrator") },
                    ContractOfRefWorker = _context.Contracts.First(c => c.Label == "Festanstellung"),
                    DepartmentOfRefWorker = _context.Departments.First(d => d.Name == "People & Culture"),
                    AssignmentTemplates = new List<AssignmentTemplate>
                    {
                        new AssignmentTemplate { Title = "GSuite Account erstellen", AssigneeType = AssigneeType.ROLES, DueIn = _context.DueTimes.First(dt => dt.Label == "ASAP") },
                        new AssignmentTemplate { Title = "Hardware", AssigneeType = AssigneeType.WORKER_OF_REF, AssignedRole = _context.Roles.First(r => r.Name == "IT"), DueIn = _context.DueTimes.First(dt => dt.Label == "ASAP") }
                    }
                };
                _context.ProcessTemplates.Add(processTemplate);
                _context.SaveChanges();
            }

            // Processes
            if (!_context.Processes.Any())
            {
                var process = new Process
                {
                    Title = "Neuzugang HR",
                    ContractOfRefWorker = _context.Contracts.First(c => c.Label == "Trainee"),
                    DepartmentOfRefWorker = _context.Departments.First(d => d.Name == "People & Culture"),
                    Supervisor = _mockUserManager.Object.FindByEmailAsync("koenig@gmx.de").Result,
                    WorkerOfReference = _mockUserManager.Object.FindByEmailAsync("bill@gmx.de").Result,
                    DueDate = new DateTime(2024, 9, 13),
                    Assignments = new List<Assignment>
                    {
                        new Assignment { Title = "GSuite Account erstellen", AssigneeType = AssigneeType.ROLES, DueDate = new DateTime(2024, 9, 13) },
                        new Assignment { Title = "Hardware", AssigneeType = AssigneeType.WORKER_OF_REF, AssignedRole = _context.Roles.First(r => r.Name == "IT"), DueDate = new DateTime(2024, 9, 13) }
                    }
                };
                _context.Processes.Add(process);
                _context.SaveChanges();
            }
        }
*/
    
        /// <summary>
        /// Disposes of the ProcessCreationTest object, when the test has finished.
        /// </summary>
        /// <owner>Nicolai Kirndorfer</owner>
        }
        public void Dispose() 
        {
            _driver.Quit(); 
            _driver.Dispose(); 
        }
        
        
        /// <summary>
        /// Test for "New Employee see its Assignmnet List"
        /// </summary>
        /// <owner>Nicolai Kirndorfer</owner>
        //[Fact]
        public void New_Employee_Change_AssignmentStatus()
        {
            _driver.Navigate().GoToUrl("https://localhost:7003/");

            // _driver.FindElement(By.Id("restoreTestDataBtn")).Click();
            // _driver.FindElement(By.Id("restoreTestDataConfirmBtn")).Click();

            _driver.Navigate().GoToUrl(URI);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));

            Assert.Equal("- SoPro24Team06", _driver.Title); 

            var inputElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("login-submit")));

            _driver.FindElement(By.Id("Email")).SendKeys("admin@gmx.de");
            _driver.FindElement(By.Id("Password")).SendKeys("Admin1!");
            _driver.FindElement(By.Id("login-submit")).Click();
        
            _driver.Navigate().GoToUrl(URI);

            Assert.Equal("- SoPro24Team06", _driver.Title);    

            inputElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("allAssignmentsButton")));
            _driver.FindElement(By.Id("allAssignmentsButton")).Click();

            // Collect the Assignments on the view
            inputElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.TagName("tbody")));
            var assignmentsTable = _driver.FindElement(By.Id("allAssignmentsTable"));
            var rows = assignmentsTable.FindElements(By.CssSelector("tbody tr"));

            var editButton = rows[0].FindElement(By.Id("editAssignment")); // Hier sollte der korrekte Selektor verwendet werden
            editButton.Click();

            // Erstes Mal in die Edit View 
            inputElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("saveEditAssignment")));

            var dropdownElement = _driver.FindElement(By.Id("assignmentStatusDropdown"));
            var selectElement = new SelectElement(dropdownElement);

            selectElement.SelectByText("IN_PROGRESS");

            _driver.FindElement(By.Id("saveEditAssignment")).Click();

            Assert.Equal("In Bearbeitung", rows[0].FindElement(By.Id("assignmentStatusCheck")).Text);   

            inputElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.TagName("tbody")));

            assignmentsTable = _driver.FindElement(By.Id("allAssignmentsTable"));
            rows = assignmentsTable.FindElements(By.CssSelector("tbody tr"));

            editButton = rows[0].FindElement(By.Id("editAssignment")); // Hier sollte der korrekte Selektor verwendet werden
            editButton.Click();

            // Zweites Mal in die Edit View

            inputElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("saveEditAssignment")));

            dropdownElement = _driver.FindElement(By.Id("assignmentStatusDropdown"));
            selectElement = new SelectElement(dropdownElement);

            selectElement.SelectByText("DONE");

            _driver.FindElement(By.Id("saveEditAssignment")).Click();

            inputElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.TagName("tbody")));

            Assert.Equal("Fertig", rows[0].FindElement(By.Id("assignmentStatusCheck")).Text);   







            

            /*List<string> assignments = new List<string>();
            foreach (var row in rows)
            {
                var columns = row.FindElements(By.CssSelector("td"));
                var description = columns[0].FindElement(By.TagName("a")).Text;
                assignments.Add(description);
            }

            // Check the correct number of Assignments 
            Assert.Equal(6, assignments.Count);

            // Check the correct Descriptions of Assignments
            List<string> referenceData = new List<string>{
                "(T) Richte eine vernünftige GitConfig ein.",
                "(T) Füge deinen SSH-Schlüssel in dein GitLab-Profil ein.",
                "Stelle sicher, dass du deine Passwörter nicht mit der Google Chrome Cloud synchronisierst.",
                "Info: Bitte verwende keine privaten Geräte für die Arbeit, außer bei den im Lauf des Onboardings ausdrücklich genannten Ausnahmen.",
                "Rückfragen bei ISB",
                "Lies dir durch und merk dir, wie wir mit Passwörtern, Browsererweiterungen und Sicherheitsgrundlagen umgehen."
            };
            foreach (var assignment in assignments)
            {
                Assert.Contains(assignment, referenceData);
            }*/
        }
    }
}