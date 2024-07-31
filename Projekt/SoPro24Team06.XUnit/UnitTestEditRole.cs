//-------------------------
// Author: Michael Adolf
//-------------------------

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SoPro24Team06.Controllers;
using SoPro24Team06.Models;
using SoPro24Team06.Data;
using Xunit;

namespace SoPro24Team06.Tests
{
    public class AdministrationControllerEditRoleTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<ILogger<AdministrationController>> _loggerMock;

        private readonly AdministrationController _controller;

        public AdministrationControllerEditRoleTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), 
                null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
                roleStoreMock.Object,
                null, null, null, null);

            _loggerMock = new Mock<ILogger<AdministrationController>>();

            _controller = new AdministrationController(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _loggerMock.Object,
                null // ApplicationDbContext nicht benötigt
            );
        }

        [Fact]
        public async Task EditRole_ReturnsJsonResult_Success()
        {
            // Arrange
            var oldRoleName = "OldRole";
            var newRoleName = "NewRole";
            var role = new ApplicationRole { Name = oldRoleName };
            _roleManagerMock.Setup(r => r.FindByNameAsync(oldRoleName)).ReturnsAsync(role);
            _roleManagerMock.Setup(r => r.UpdateAsync(It.IsAny<ApplicationRole>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.EditRole(oldRoleName, newRoleName) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var success = (bool)result.Value.GetType().GetProperty("success").GetValue(result.Value);
            Assert.True(success);
        }

        [Fact]
        public async Task EditRole_ReturnsJsonResult_RoleNotFound()
        {
            // Arrange
            var oldRoleName = "NonExistingRole";
            var newRoleName = "NewRole";
            _roleManagerMock.Setup(r => r.FindByNameAsync(oldRoleName)).ReturnsAsync((ApplicationRole)null);

            // Act
            var result = await _controller.EditRole(oldRoleName, newRoleName) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var success = (bool)result.Value.GetType().GetProperty("success").GetValue(result.Value);
            var error = result.Value.GetType().GetProperty("error").GetValue(result.Value).ToString();
            Assert.False(success);
            Assert.Equal("Role not found.", error);
        }
    }
}
