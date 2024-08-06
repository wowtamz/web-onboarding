//-------------------------
// Author: Kevin Tornquist
//-------------------------


using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SoPro24Team06.Controllers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using Xunit;

namespace SoPro24Team06.Tests
{
    public class ProcessTemplateControllerTest
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly ApplicationDbContext _context;
        private readonly List<ApplicationUser> _users;
        private readonly List<ApplicationRole> _roles;
        private readonly List<IdentityUserRole<string>> _userRoles;

        public ProcessTemplateControllerTest()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            var roleStore = new Mock<IRoleStore<ApplicationRole>>();
            _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
                roleStore.Object,
                null,
                null,
                null,
                null
            );

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            _users = new List<ApplicationUser>();
            _roles = new List<ApplicationRole>();
            _userRoles = new List<IdentityUserRole<string>>();

            // Set up mock Managers to persist storage

            // Set up the mock UserManager to use the list for storage
            _mockUserManager
                .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser>(user => _users.Add(user));

            _mockUserManager
                .Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(
                    (string fullname) => _users.FirstOrDefault(u => u.FullName == fullname)
                );

            _mockUserManager
                .Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string userId) => _users.FirstOrDefault(u => u.Id == userId));

            _mockUserManager
                .Setup(um =>
                    um.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>())
                )
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, IEnumerable<string>>(
                    (user, roles) =>
                    {
                        foreach (var role in roles)
                        {
                            var applicationRole = _roles.FirstOrDefault(r => r.Name == role);
                            if (applicationRole != null)
                            {
                                _userRoles.Add(
                                    new IdentityUserRole<string>
                                    {
                                        UserId = user.Id,
                                        RoleId = applicationRole.Id
                                    }
                                );
                            }
                        }
                    }
                );

            _mockUserManager
                .Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(
                    (ApplicationUser user) =>
                    {
                        if (user == null)
                        {
                            return new List<string>(); // Return an empty list if user is null
                        }

                        var userRoleIds = _userRoles
                            .Where(ur => ur.UserId == user.Id)
                            .Select(ur => ur.RoleId)
                            .ToList();
                        var userRoleNames = _roles
                            .Where(r => userRoleIds.Contains(r.Id))
                            .Select(r => r.Name)
                            .ToList();
                        return userRoleNames;
                    }
                );

            // Set up the mock RoleManager to use the list for storage
            _mockRoleManager
                .Setup(rm => rm.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationRole>(role => _roles.Add(role));

            _mockRoleManager
                .Setup(rm => rm.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string roleName) => _roles.FirstOrDefault(r => r.Name == roleName));

            SeedData();
        }

        private void SeedData()
        {
            // Create roles
            var adminRole = new ApplicationRole
            {
                Name = "Administrator",
                NormalizedName = "ADMIN"
            };
            var userRole = new ApplicationRole { Name = "User", NormalizedName = "USER" };

            _context.Roles.Add(adminRole);
            _context.Roles.Add(userRole);
            _context.SaveChanges();

            // Add Roles to mockroleManager for consistency

            _mockRoleManager.Object.CreateAsync(adminRole);
            _mockRoleManager.Object.CreateAsync(userRole);

            // Create users
            var adminUser = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FullName = "Administrator"
            };
            var normalUser = new ApplicationUser
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                FullName = "User"
            };

            _context.Users.Add(adminUser);
            _context.Users.Add(normalUser);
            _context.SaveChanges();

            // Add Users to mockUserManager for consistency
            _mockUserManager.Object.CreateAsync(adminUser);
            _mockUserManager.Object.CreateAsync(normalUser);

            // Assign roles to users
            _context.UserRoles.Add(
                new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = adminRole.Id }
            );
            _context.UserRoles.Add(
                new IdentityUserRole<string> { UserId = normalUser.Id, RoleId = userRole.Id }
            );
            _context.SaveChanges();

            // Assign Roles using mockUserManager for consistency
            _mockUserManager.Object.AddToRolesAsync(adminUser, new List<string> { adminRole.Name });
            _mockUserManager.Object.AddToRolesAsync(normalUser, new List<string> { userRole.Name });

            //Create contracts

            var contract1 = new Contract { Label = "Contract 1" };
            var contract2 = new Contract { Label = "Contract 2" };

            _context.Contracts.Add(contract1);
            _context.Contracts.Add(contract2);
            _context.SaveChanges();

            //Create Departments

            var department1 = new Department { Name = "Department 1" };
            var department2 = new Department { Name = "Department 2" };

            _context.Departments.Add(department1);
            _context.Departments.Add(department2);
            _context.SaveChanges();

            //Create DueTimes

            var dueTime1 = new DueTime("ASAP", 1, 0, 0);
            var dueTime2 = new DueTime("1 Week", 1, 0, 0);

            _context.DueTimes.Add(dueTime1);
            _context.DueTimes.Add(dueTime2);
            _context.SaveChanges();

            //Create AssignmentTemplate

            var assignmentTemplate = new AssignmentTemplate(
                "title",
                "instruction",
                dueTime1,
                new List<Department> { },
                new List<Contract> { },
                AssigneeType.SUPERVISOR,
                userRole,
                null
            );

            _context.AssignmentTemplates.Add(assignmentTemplate);
            _context.SaveChanges();
        }

        public async Task<HttpContext> CreateMockHttpContextForUser(ApplicationUser user)
        {
            var usersRoles = await _mockUserManager.Object.GetRolesAsync(user);

            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();

            mockClaimsPrincipal
                .Setup(cp => cp.FindFirst(It.IsAny<string>()))
                .Returns<string>(type =>
                    type == ClaimTypes.NameIdentifier
                        ? new Claim(ClaimTypes.NameIdentifier, user.Id)
                        : null
                );

            mockClaimsPrincipal
                .Setup(cp => cp.IsInRole(It.IsAny<string>()))
                .Returns((string role) => usersRoles.Select(r => r).ToList().Contains(role));

            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);

            return mockHttpContext.Object;
        }

        // Creates ProcessController with all mocked resources ready for testing
        // user = the user which is accessing the methods from the controller
        public async Task<ProcessTemplateController> CreateProcessTemplateController(
            ApplicationUser user
        )
        {
            var logger = new LoggerFactory().CreateLogger<ProcessTemplateController>();

            ProcessTemplateController controller = new ProcessTemplateController(
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _context,
                logger
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = await CreateMockHttpContextForUser(user)
                }
            };

            return controller;
        }

        [Fact]
        public async Task UserCannotCreateProcessTemplate()
        {
            var user = await _mockUserManager.Object.FindByNameAsync("User");
            var controller = await CreateProcessTemplateController(user);

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task EditProcessTemplateWithStartedProcessesRedirect()
        {
            var user = await _mockUserManager.Object.FindByNameAsync("User");
            var usersRoles = await _mockUserManager.Object.GetRolesAsync(user);

            Assert.NotNull(usersRoles);
            ApplicationRole roleWithAccess = await _mockRoleManager.Object.FindByNameAsync(
                usersRoles.First()
            );

            var contract = _context.Contracts.FirstOrDefault();
            var department = _context.Departments.FirstOrDefault();

            var assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault();

            var controller = await CreateProcessTemplateController(user);
            var template = new ProcessTemplate
            {
                Title = "Test Template",
                RolesWithAccess = new List<ApplicationRole> { roleWithAccess },
                ContractOfRefWorker = contract,
                DepartmentOfRefWorker = department,
                AssignmentTemplates = new List<AssignmentTemplate> { assignmentTemplate },
                Description = "NONE"
            };
            _context.ProcessTemplates.Add(template);
            _context.SaveChanges();

            var process = _context.Processes.Add(
                new Process
                {
                    Title = "Test Process",
                    ContractOfRefWorker = contract,
                    Assignments = new List<Assignment> { },
                    Description = "",
                    Supervisor = user,
                    DepartmentOfRefWorker = department,
                    IsArchived = false,
                    WorkerOfReference = user,
                }
            );
            _context.SaveChanges();

            var result = await controller.Edit(template.Id);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task InvalidCreateModel()
        {
            var supervisor = await _mockUserManager.Object.FindByNameAsync("Administrator");
            var usersRoles = await _mockUserManager.Object.GetRolesAsync(supervisor);
            var controller = await CreateProcessTemplateController(supervisor);

            var contract = _context.Contracts.FirstOrDefault();
            var department = _context.Departments.FirstOrDefault();

            var assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault();

            var templateViewModel = new ProcessTemplateViewModel
            {
                Title = "New Process Template",
                Description = "Template Description",
                ContractOfRefWorkerId = contract.Id,
                DepartmentOfRefWorkerId = department.Id,
                RolesWithAccess = new List<string> { "Administrator" },
                SelectedAssignmentTemplateIds = new List<int> { assignmentTemplate.Id }
            };

            var result = await controller.Create(templateViewModel);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectResult = result as RedirectToActionResult;
            Assert.Equal("Create", redirectResult.ActionName);
        }

        [Fact]
        public async Task SupervisorCanEditProcessTemplate()
        {
            var supervisor = await _mockUserManager.Object.FindByNameAsync("Administrator");
            var controller = await CreateProcessTemplateController(supervisor);

            var existingTemplate = _context.ProcessTemplates.First();
            var updatedTitle = "Updated Process Template";
            var updatedDescription = "Updated Description";

            var templateViewModel = new ProcessTemplateViewModel
            {
                Id = existingTemplate.Id,
                Title = updatedTitle,
                Description = updatedDescription,
                ContractOfRefWorkerId = existingTemplate.ContractOfRefWorker.Id,
                DepartmentOfRefWorkerId = existingTemplate.DepartmentOfRefWorker.Id,
                SelectedAssignmentTemplateIds = existingTemplate
                    .AssignmentTemplates.Select(at => at.Id)
                    .ToList(),
                RolesWithAccess = new List<string> { "Administrator" }
            };

            var result = await controller.Edit(templateViewModel);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectResult = result as RedirectToActionResult;
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task SupervisorCanDeleteProcessTemplate()
        {
            var supervisor = await _mockUserManager.Object.FindByNameAsync("Administrator");
            var controller = await CreateProcessTemplateController(supervisor);

            var templateToDelete = _context.ProcessTemplates.First();

            var result = await controller.Delete(templateToDelete.Id);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectResult = result as RedirectToActionResult;
            Assert.Equal("Index", redirectResult.ActionName);

            var deletedTemplate = _context.ProcessTemplates.Find(templateToDelete.Id);
            Assert.Null(deletedTemplate);
        }

        [Fact]
        public async Task UserCannotDeleteProcessTemplate()
        {
            var user = await _mockUserManager.Object.FindByNameAsync("User");
            var controller = await CreateProcessTemplateController(user);

            var templateToDelete = _context.ProcessTemplates.First();

            var result = await controller.Delete(templateToDelete.Id);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task AdminCanViewProcessTemplateDetails()
        {
            var admin = await _mockUserManager.Object.FindByNameAsync("Administrator");
            var controller = await CreateProcessTemplateController(admin);

            var contract = _context.Contracts.FirstOrDefault();
            var department = _context.Departments.FirstOrDefault();

            var assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault();

            var template = new ProcessTemplate
            {
                Title = "Test Template",
                RolesWithAccess = new List<ApplicationRole> { },
                ContractOfRefWorker = contract,
                DepartmentOfRefWorker = department,
                AssignmentTemplates = new List<AssignmentTemplate> { assignmentTemplate },
                Description = "NONE"
            };
            var res = _context.ProcessTemplates.Add(template);
            _context.SaveChanges();

            var templateToView = _context.ProcessTemplates.FirstOrDefault(x =>
                x.Id == res.Entity.Id
            );

            var result = await controller.Detail(templateToView.Id);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.IsType<DetailProcessTemplateViewModel>(viewResult.Model);

            var model = viewResult.Model as DetailProcessTemplateViewModel;
            Assert.NotNull(model);
            Assert.Equal(templateToView.Title, model.Title);
            Assert.Equal(templateToView.Description, model.Description);
        }
    }
}
