//-------------------------
// Author: Vincent Steiner
//-------------------------
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SoPro24Team06.Containers;
using SoPro24Team06.Controllers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using SoPro24Team06.Interfaces;
using SoPro24Team06.XUnit;
using SoPro24Team06.Interfaces;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace SoPro24Team06.XUnit
{
    public class AssignmentTemplateController_UnitTest
   {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly ApplicationDbContext _context;
        private readonly List<ApplicationUser> _users;
        private readonly List<ApplicationRole> _roles;
        private readonly List<IdentityUserRole<string>> _userRoles;
        private int _assignmentTemplateId;
        private int _processTemplateId;


        public AssignmentTemplateController_UnitTest()
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
                        return new List<string>(); 
                    }

                    var userRoleIds = _userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
                    var userRoleNames = _roles.Where(r => userRoleIds.Contains(r.Id)).Select(r => r.Name).ToList();
                    return userRoleNames;
                });
            
            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationRole>(role => _roles.Add(role));

            _mockRoleManager.Setup(rm => rm.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string roleName) => _roles.FirstOrDefault(r => r.Name == roleName));

            
            SeedData();
        }
        
        private void SeedData()
        {
            var adminRole = new ApplicationRole { Name = "Administrator" };
            var userRole = new ApplicationRole { Name = "User", NormalizedName = "USER" };

            if (!_context.Roles.Any(r => r.Name == adminRole.Name))
            {
                _context.Roles.Add(adminRole);
            }

            if (!_context.Roles.Any(r => r.Name == userRole.Name))
            {
                _context.Roles.Add(userRole);
            }
            _context.SaveChanges();

            _mockRoleManager.Object.CreateAsync(adminRole);
            _mockRoleManager.Object.CreateAsync(userRole);

            var adminUser = new ApplicationUser { UserName = "admin@example.com", Email = "admin@example.com", FullName = "Administrator" };
            var normalUser = new ApplicationUser { UserName = "user@example.com", Email = "user@example.com", FullName = "User" };

            if (!_context.Users.Any(u => u.UserName == adminUser.UserName))
            {
                _context.Users.Add(adminUser);
            }

            if (!_context.Users.Any(u => u.UserName == normalUser.UserName))
            {
                _context.Users.Add(normalUser);
            }
            _context.SaveChanges();

            _mockUserManager.Object.CreateAsync(adminUser);
            _mockUserManager.Object.CreateAsync(normalUser);

            if (!_context.UserRoles.Any(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id))
            {
                _context.UserRoles.Add(new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = adminRole.Id });
            }

            if (!_context.UserRoles.Any(ur => ur.UserId == normalUser.Id && ur.RoleId == userRole.Id))
            {
                _context.UserRoles.Add(new IdentityUserRole<string> { UserId = normalUser.Id, RoleId = userRole.Id });
            }
            _context.SaveChanges();

            _mockUserManager.Object.AddToRolesAsync(adminUser, new List<string> { adminRole.Name });
            _mockUserManager.Object.AddToRolesAsync(normalUser, new List<string> { userRole.Name });

            var contract1 = new Contract { Label = "Festanstellung" };
            var contract2 = new Contract { Label = "Trainee" };

            if (!_context.Contracts.Any(c => c.Label == contract1.Label))
            {
                _context.Contracts.Add(contract1);
            }

            if (!_context.Contracts.Any(c => c.Label == contract2.Label))
            {
                _context.Contracts.Add(contract2);
            }
            _context.SaveChanges();

            var department1 = new Department { Name = "IT" };
            var department2 = new Department { Name = "Sales" };

            if (!_context.Departments.Any(d => d.Name == department1.Name))
            {
                _context.Departments.Add(department1);
            }

            if (!_context.Departments.Any(d => d.Name == department2.Name))
            {
                _context.Departments.Add(department2);
            }
            _context.SaveChanges();

            var dueTime1 = new DueTime { Label = "ASAP", Days = 0, Weeks = 0, Months = 0 };

            if (!_context.DueTimes.Any(dt => dt.Label == dueTime1.Label))
            {
                _context.DueTimes.Add(dueTime1);
            }
            _context.SaveChanges();

            List<AssignmentTemplate> atList = new List<AssignmentTemplate>();

            List<ApplicationRole> rolesList = new List<ApplicationRole> { adminRole };

            if (!_context.ProcessTemplates.Any(pt => pt.Title == "Eis essen"))
            {
                var processTemplate = new ProcessTemplate(123, "Eis essen", "Schnell bevor es schmilzt", atList, contract1, department1, rolesList);
                _context.ProcessTemplates.Add(processTemplate);
                _context.SaveChanges();
                _processTemplateId = processTemplate.Id;
            }

            var assignmentTemplate = new AssignmentTemplate("title", "instruction", dueTime1, new List<Department> { },
                new List<Contract> { }, AssigneeType.SUPERVISOR, userRole, 123);

            if (!_context.AssignmentTemplates.Any(at => at.Title == "title"))
            {
                _context.AssignmentTemplates.Add(assignmentTemplate);
                _context.SaveChanges();
                _assignmentTemplateId = assignmentTemplate.Id;
            }
        }
        [Fact]
        public async Task CreateAssignmentTemplate_Test(){ // Tested die Create Methode im AssignmentTemplateController

            var logger = new LoggerFactory().CreateLogger<AssignmentTemplateController>();
            
            var atController = new AssignmentTemplateController(_context, _mockUserManager.Object, _mockRoleManager.Object, logger);
            var tempData = new Mock<ITempDataDictionary>();
            atController.TempData = tempData.Object;

            var model = new CreateEditAssignmentTemplateViewModel(
                1,
                12,
                "Projektplan",
                "Dies sind die Anweisungen.",
                "ASAP",
                null, 
                null, 
                "SUPERVISOR",
                "Administrator",
                5,
                1,
                2,
                "Nach:"
            );

            var result = await atController.Create(model);

            Assert.NotNull(result);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.Equal("ProcessTemplate", redirectResult.ControllerName);
            Assert.Equal(12, redirectResult.RouteValues["id"]);

        }
        [Fact]
        public async Task CreateAssignmentTemplate_UserInputDueTime_Test(){ // Tested die Create Methode im AssignmentTemplateController mit Benutzerdefinierter DueTime

            var logger = new LoggerFactory().CreateLogger<AssignmentTemplateController>();
            
            var atController = new AssignmentTemplateController(_context, _mockUserManager.Object, _mockRoleManager.Object, logger);
            var tempData = new Mock<ITempDataDictionary>();
            atController.TempData = tempData.Object;

            var model = new CreateEditAssignmentTemplateViewModel(
                1,
                12,
                "Projektplan",
                "Dies sind die Anweisungen.",
                "Benutzerdefiniert",
                null, 
                null, 
                "WORKER_OF_REF",
                "Administrator",
                5,
                1,
                2,
                "Nach:"
            );

            var result = await atController.Create(model);

            Assert.NotNull(result);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.Equal("ProcessTemplate", redirectResult.ControllerName);
            Assert.Equal(12, redirectResult.RouteValues["id"]);

        }
        [Fact]
        public async Task EditAssignmentTemplate_Test(){ // Tested die Edit Methode im AssignmentTemplateController

            var logger = new LoggerFactory().CreateLogger<AssignmentTemplateController>();
            
            var atController = new AssignmentTemplateController(_context, _mockUserManager.Object, _mockRoleManager.Object, logger);
            var tempData = new Mock<ITempDataDictionary>();
            atController.TempData = tempData.Object;

            var model = new CreateEditAssignmentTemplateViewModel(
                1,
                12,
                "Projektplan",
                "Dies sind die Anweisungen.",
                "ASAP",
                null, 
                null, 
                "SUPERVISOR",
                "Administrator",
                5,
                1,
                2,
                "Nach:"
            );

            var result = await atController.Edit(model);

            Assert.NotNull(result);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.Equal("ProcessTemplate", redirectResult.ControllerName);
            Assert.Equal(12, redirectResult.RouteValues["id"]);

        }
        [Fact]
        public async Task DeleteAssignmentTemplate_Test() // Tested die Delete Methode im AssignmentTemplateController
        {
            var user = await _mockUserManager.Object.FindByNameAsync("Administrator");
            var _usersRoles = await _mockUserManager.Object.GetRolesAsync(user);

            var logger = new LoggerFactory().CreateLogger<AssignmentTemplateController>();

            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();

            mockClaimsPrincipal.Setup(cp => cp.IsInRole(It.IsAny<string>()))
                .Returns((string role) => _usersRoles.Contains(role));
            mockHttpContext.Setup(ctx => ctx.User.FindFirst(It.IsAny<string>()))
                .Returns(new Claim(ClaimTypes.NameIdentifier, user.Id));
            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);

            var atController = new AssignmentTemplateController(_context, _mockUserManager.Object, _mockRoleManager.Object, logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            var tempData = new Mock<ITempDataDictionary>();
            atController.TempData = tempData.Object;

            var result = await atController.Delete(_assignmentTemplateId);

            Assert.NotNull(result);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Detail", redirectResult.ActionName);
            Assert.Equal("ProcessTemplate", redirectResult.ControllerName);
            Assert.Equal(123, redirectResult.RouteValues["id"]);
        }
    }
}

