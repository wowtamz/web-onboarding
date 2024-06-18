using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoPro24Team06.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoPro24Team06.Controllers
{
    // Nur sichtbar mit Admin Rolle
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = await GetUserRoles(user)
                };
                userRolesViewModel.Add(thisViewModel);
            }

            var model = new UserManagementViewModel
            {
                CreateUserViewModel = new CreateUserViewModel(),
                UserRoles = userRolesViewModel
            };

            return View("~/Views/Admin/UserManagement.cshtml", model); 
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.CreateUserViewModel.Email,
                    Email = model.CreateUserViewModel.Email,
                    FullName = model.CreateUserViewModel.FullName
                };

                var result = await _userManager.CreateAsync(user, model.CreateUserViewModel.Password);

                if (result.Succeeded)
                {
                    // User ist default rolle
                    await _userManager.AddToRoleAsync(user, "User");
                    return RedirectToAction("UserManagement"); 
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = await GetUserRoles(user)
                };
                userRolesViewModel.Add(thisViewModel);
            }

            model.UserRoles = userRolesViewModel;
            return View("~/Views/Admin/UserManagement.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.Email != "admin@example.com")
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("UserManagement");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToAction("UserManagement");
        }

        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }
    }
}
