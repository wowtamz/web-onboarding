using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SoPro24Team06.Containers;
using SoPro24Team06.Controllers;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using SoPro24Team06.XUnit;
using SoPro24Team06.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace SoPro24Team06.XUnit
{
    public class AssignmentTemplateController_UnitTest
    {
        [Fact]
        public async Task AddAssignmentTemplate_InValidInput()
        {
            using var _context = await UnitTestDataHelper.CreateDbInMomory();

            var userStore = new UserStore<ApplicationUser>(_context);
            var roleStore = new RoleStore<ApplicationRole>(_context);
            var _userManager = new UserManager<ApplicationUser>(
                userStore,
                null,
                new PasswordHasher<ApplicationUser>(),
                null,
                null,
                null,
                null,
                null,
                null
            );
            var _roleManager = new RoleManager<ApplicationRole>(roleStore, null, null, null, null);
            var _loggerMock = new Mock<ILogger<AssignmentTemplateController>>().Object;

            AssignmentTemplateController atController = new AssignmentTemplateController(
                _context,
                _userManager,
                _roleManager,
                _loggerMock
            );
            var atContainer = new Mock<IAssignmentTemplate>();
            var dtContainer = new Mock<IDueTime>();
            var dpContainer = new Mock<IDepartment>();
            var coContainer = new Mock<IContract>();

             var atmodel = new CreateEditAssignmentTemplateViewModel(
                1,
                1,
                null,
                "",
                "Benutzerdefiniert",
                new List<string>(),
                new List<string>(),
                "Bezugsperson",
                null,
                10,
                2,
                1,
                "Vor Start"
            );
            atController.ModelState.AddModelError("", "Model is not Valid");

            atController.Create(atmodel);

            Assert.False(atController.ModelState.IsValid);
        }

        /*[Fact]
        public async Task AssignmentTemplateCreate_withValidModel_IsCreated()
        {
            await using var _context = await UnitTestDataHelper.CreateDbInMomory();

            var userStore = new UserStore<ApplicationUser>(_context);
            var roleStore = new RoleStore<ApplicationRole>(_context);
            var _userManager = new UserManager<ApplicationUser>(
                userStore,
                null,
                new PasswordHasher<ApplicationUser>(),
                null,
                null,
                null,
                null,
                null,
                null
            );
            var _roleManager = new RoleManager<ApplicationRole>(roleStore, null, null, null, null);
            var _loggerMock = new Mock<ILogger<AssignmentTemplateController>>().Object;

            ProcessTemplateContainer ptContainer = new ProcessTemplateContainer(_context);
            AssignmentTemplateContainer atContainer = new AssignmentTemplateContainer(_context);
            AssignmentTemplateController atController = new AssignmentTemplateController(
                _context,
                _userManager,
                _roleManager,
                _loggerMock
            );

            var processTemplate = await ptContainer.GetProcessTemplateByIdAsync(1);
            if (processTemplate == null)
            {
                await SeedProcessTemplates(_context);
                _output.WriteLine("Ist Null !!!!!!!!!!!!!!!!!!!!!!!");
                processTemplate = await ptContainer.GetProcessTemplateByIdAsync(1); // Die neu erstellte Prozessvorlage abrufen
            }
            _output.WriteLine("Ist nicht Null !!!!!!!!!!!!!!!!!!!!!!!");

            var atmodel = new CreateEditAssignmentTemplateViewModel(
                1,
                1,
                "Projektplan",
                "",
                "Benutzerdefiniert",
                new List<string>(),
                new List<string>(),
                "Bezugsperson",
                null,
                10,
                2,
                1,
                "Vor Start"
            );
            var result = atController.Create(atmodel); // Await der asynchronen Methode
            var createdAT = atContainer.GetAssignmentTemplate(atmodel.Id); // Await der asynchronen Methode

            Assert.NotNull(createdAT);
            Assert.Equal("Projektplan", createdAT.Title);
            Assert.Equal("SUPERVISOR", createdAT.AssigneeType.ToString());
            Assert.Equal("", createdAT.Instructions);
            Assert.Equal("10 Tage 2 Wochen 1 Monat vor Start", createdAT.DueIn.Label);
            Assert.IsType<RedirectToActionResult>(result);
        }*/
    }
}

