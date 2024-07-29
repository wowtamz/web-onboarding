using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SoPro24Team06.Controllers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using Xunit;

namespace SoPro24Team06.Tests
{
    public class ProcessControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly ApplicationDbContext _context;
        private readonly List<ApplicationUser> _users;
        private readonly List<ApplicationRole> _roles;
        private readonly List<IdentityUserRole<string>> _userRoles;

        public ProcessControllerTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<ApplicationRole>>();
            _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(roleStore.Object, null, null, null, null);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            
            _users = new List<ApplicationUser>();
            _roles = new List<ApplicationRole>();
            _userRoles = new List<IdentityUserRole<string>>();
            
            // Set up mock Managers to persist storage
            
            // Set up the mock UserManager to use the list for storage
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser>(user => _users.Add(user));

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string fullname) => _users.FirstOrDefault(u => u.FullName == fullname));

            _mockUserManager.Setup(um => um.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, IEnumerable<string>>((user, roles) =>
                {
                    foreach (var role in roles)
                    {
                        var applicationRole = _roles.FirstOrDefault(r => r.Name == role);
                        if (applicationRole != null)
                        {
                            _userRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = applicationRole.Id });
                        }
                    }
                });

            _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync((ApplicationUser user) =>
                {
                    if (user == null)
                    {
                        return new List<string>(); // Return an empty list if user is null
                    }

                    var userRoleIds = _userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
                    var userRoleNames = _roles.Where(r => userRoleIds.Contains(r.Id)).Select(r => r.Name).ToList();
                    return userRoleNames;
                });
            
            // Set up the mock RoleManager to use the list for storage
            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationRole>(role => _roles.Add(role));

            _mockRoleManager.Setup(rm => rm.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string roleName) => _roles.FirstOrDefault(r => r.Name == roleName));
            
            SeedData();
        }
        
        private void SeedData()
        {
            // Create roles
            var adminRole = new ApplicationRole { Name = "Administrator", NormalizedName = "ADMIN" };
            var userRole = new ApplicationRole { Name = "User", NormalizedName = "USER" };

            _context.Roles.Add(adminRole);
            _context.Roles.Add(userRole);
            _context.SaveChanges();
            
            // Add Roles to mockroleManager for consistency

            _mockRoleManager.Object.CreateAsync(adminRole);
            _mockRoleManager.Object.CreateAsync(userRole);
            

            // Create users
            var adminUser = new ApplicationUser { UserName = "admin@example.com", Email = "admin@example.com", FullName = "Administrator" };
            var normalUser = new ApplicationUser { UserName = "user@example.com", Email = "user@example.com", FullName = "User" };

            _context.Users.Add(adminUser);
            _context.Users.Add(normalUser);
            _context.SaveChanges();
            
            // Add Users to mockUserManager for consistency
            _mockUserManager.Object.CreateAsync(adminUser);
            _mockUserManager.Object.CreateAsync(normalUser);

            // Assign roles to users
            _context.UserRoles.Add(new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = adminRole.Id });
            _context.UserRoles.Add(new IdentityUserRole<string> { UserId = normalUser.Id, RoleId = userRole.Id });
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
            
            var assignmentTemplate = new AssignmentTemplate("title", "instruction", dueTime1, new List<Department> { },
                new List<Contract> { }, AssigneeType.SUPERVISOR, userRole, null);
            
            _context.AssignmentTemplates.Add(assignmentTemplate);
            _context.SaveChanges();

        }

        [Fact]
        public async Task IndexReturnProcessesNotAdmin()
        {
            var user = await _mockUserManager.Object.FindByNameAsync("User");

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(ctx => ctx.User.FindFirst(It.IsAny<string>())).Returns(new Claim(ClaimTypes.NameIdentifier, user.Id));

            var controller = new ProcessController(_context, _mockUserManager.Object, _mockRoleManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            
            var result = await controller.Index();

            // Asserts
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProcessListViewModel>(viewResult.Model);
            Assert.NotNull(viewResult.ViewData["MyProcesses"]);
            Assert.NotNull(viewResult.ViewData["ArchivedProcesses"]);
            Assert.Null(viewResult.ViewData["AllProcesses"]);
            Assert.Null(viewResult.ViewData["AllArchivedProcesses"]);

        }
        
        [Fact]
        public async Task IndexReturnProcessesAdmin()
        {
            var user = await _mockUserManager.Object.FindByNameAsync("Administrator");
            var usersRoles = await _mockUserManager.Object.GetRolesAsync(user);

            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            
            mockClaimsPrincipal.Setup(cp => cp.IsInRole(It.IsAny<string>()))
                .Returns((string role) => usersRoles.Select(r => r).ToList().Contains(role));
            
            mockHttpContext.Setup(ctx => ctx.User.FindFirst(It.IsAny<string>()))
                .Returns(new Claim(ClaimTypes.NameIdentifier, user.Id));
            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);

            var controller = new ProcessController(_context, _mockUserManager.Object, _mockRoleManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            var result = await controller.Index();

            IList<string> roles = await _mockUserManager.Object.GetRolesAsync(user);

            // Asserts
            var viewResult = Assert.IsType<ViewResult>(result);
            
            Assert.NotNull(viewResult.ViewData["MyProcesses"]);
            Assert.NotNull(viewResult.ViewData["ArchivedProcesses"]);
            Assert.NotNull(viewResult.ViewData["AllProcesses"]);
            Assert.NotNull(viewResult.ViewData["AllArchivedProcesses"]);
        }

        
        [Fact]
        public async Task StartProcessWithValidProcessTemplate_WithAccess()
        {
            var user = await _mockUserManager.Object.FindByNameAsync("User");
            var usersRoles = (await _mockUserManager.Object.GetRolesAsync(user)).ToList();
            ApplicationRole mockRoleWithAccess = await _mockRoleManager.Object.FindByNameAsync(usersRoles.ToList().First());
            
            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            var roles = _context.Roles.Select(r => r.Name).ToList();
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
            var allClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id) }.Concat(roleClaims);

            mockClaimsPrincipal.Setup(cp => cp.Claims).Returns(allClaims);
            mockClaimsPrincipal.Setup(cp => cp.IsInRole(It.IsAny<string>()))
                .Returns((string role) => roles.Contains(role));
            
            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);
            
            

            var contract = _context.Contracts.FirstOrDefault();
            var department = _context.Departments.FirstOrDefault();

             var assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault();
            
            var template = new ProcessTemplate {Title = "Test Template", RolesWithAccess = new List<ApplicationRole> {mockRoleWithAccess}, ContractOfRefWorker = contract, DepartmentOfRefWorker = department, AssignmentTemplates = new List<AssignmentTemplate> {assignmentTemplate}, Description = "NONE"};
            _context.ProcessTemplates.Add(template);
            _context.SaveChanges();

            var controller = new ProcessController(_context, _mockUserManager.Object, _mockRoleManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            
            var result = await controller.Start(template.Id) as ViewResult;
            
            Assert.IsType<ViewResult>(result);
            
            Assert.NotNull(result);
            Assert.IsType<StartProcessViewModel>(result.Model);
            StartProcessViewModel startProcessViewModel = result.Model as StartProcessViewModel;
            
            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal(startProcessViewModel.Template, template);
            Assert.NotNull(startProcessViewModel.Template);
            Assert.Equal(startProcessViewModel.Template.Id, template.Id);
            Assert.Equal(startProcessViewModel.Template.Title, "Test Template");
            Assert.Equal(startProcessViewModel.Template.Description, "NONE");
            Assert.Equal(startProcessViewModel.Template.AssignmentTemplates.First(), assignmentTemplate);
            Assert.Equal(startProcessViewModel.Template.ContractOfRefWorker, contract);
            Assert.Equal(startProcessViewModel.Template.DepartmentOfRefWorker, department);
            
        }
        
        [Fact]
        public async Task StartProcessWithValidProcessTemplate_WithoutAccess()
        {
            var user = await _mockUserManager.Object.FindByNameAsync("User");
            var usersRoles = await _mockUserManager.Object.GetRolesAsync(user);
            ApplicationRole mockRoleWithAccess = await _mockRoleManager.Object.FindByNameAsync(usersRoles.ToList().First());

            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            
            mockClaimsPrincipal.Setup(cp => cp.IsInRole(It.IsAny<string>()))
                .Returns((string role) => usersRoles.ToList().Select(r => r).ToList().Contains(role));
            mockHttpContext.Setup(ctx => ctx.User.FindFirst(It.IsAny<string>())).Returns(new Claim(ClaimTypes.NameIdentifier, user.Id));
            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);

            var contract = _context.Contracts.FirstOrDefault();
            var department = _context.Departments.FirstOrDefault();

             var assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault();
            
            var template = new ProcessTemplate {Title = "Test Template", RolesWithAccess = new List<ApplicationRole> {}, ContractOfRefWorker = contract, DepartmentOfRefWorker = department, AssignmentTemplates = new List<AssignmentTemplate> {assignmentTemplate}, Description = "NONE"};
            _context.ProcessTemplates.Add(template);
            _context.SaveChanges();

            var controller = new ProcessController(_context, _mockUserManager.Object, _mockRoleManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            
            var result = await controller.Start(template.Id);
            
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            
        }
        
        [Fact]
        public async Task StartProcessWithInvalidProcessTemplate()
        {
            //var admin = await _mockUserManager.Object.FindByNameAsync("Administrator");
            var user = await _mockUserManager.Object.FindByNameAsync("User");

            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            
            mockClaimsPrincipal.Setup(cp => cp.IsInRole(It.IsAny<string>()))
                .Returns((string role) => _roles.Select(r => r.Name).ToList().Contains(role));
            mockHttpContext.Setup(ctx => ctx.User.FindFirst(It.IsAny<string>())).Returns(new Claim(ClaimTypes.NameIdentifier, user.Id));
            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);

            var contract = _context.Contracts.FirstOrDefault();
            var department = _context.Departments.FirstOrDefault();

             var assignmentTemplate = _context.AssignmentTemplates.FirstOrDefault();
            
            var template = new ProcessTemplate {Title = "Test Template", RolesWithAccess = new List<ApplicationRole>(), ContractOfRefWorker = contract, DepartmentOfRefWorker = department, AssignmentTemplates = new List<AssignmentTemplate> {assignmentTemplate}, Description = "NONE"};
            _context.ProcessTemplates.Add(template);
            _context.SaveChanges();
            

            var controller = new ProcessController(_context, _mockUserManager.Object, _mockRoleManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            var result = await controller.Start(0);
            
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            
        }
    }
}

