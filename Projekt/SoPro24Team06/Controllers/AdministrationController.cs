using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    // Nur sichtbar mit Admin Rolle
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        private readonly ILogger<AdministrationController> _logger;

        public AdministrationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AdministrationController> logger
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
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
                    Roles = await GetUserRoles(user),
                    LockoutEnd = user.LockoutEnd
                };
                userRolesViewModel.Add(thisViewModel);
            }

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var model = new AdministrationViewModel
            {
                CreateUserViewModel = new CreateUserViewModel { Roles = roles },
                UserRoles = userRolesViewModel
            };

            return View("~/Views/Administration/Index.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var model = new CreateUserViewModel { Roles = roles };
            return View("~/Views/Administration/CreateUser.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    LockoutEnd =
                        model.Status == UserStatus.LOCKED
                            ? DateTimeOffset.UtcNow.AddYears(100)
                            : (DateTimeOffset?)null
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.FullName} created.");
                    foreach (var role in model.SelectedRoles)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                        _logger.LogInformation($"User {user.FullName} added to role {role}.");
                    }
                    TempData["UserCreateMessage"] = $"Nutzer {user.FullName} erstellt";
                    return RedirectToAction("CreateUser");
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error creating user: {error.Description}");
                    ModelState.AddModelError("", error.Description);
                }
            }

            model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(); // Re-populate roles in case of an error
            return View("~/Views/Administration/CreateUser.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.Email != "admin@example.com")
            {
                _logger.LogInformation($"Deleting user {user.FullName}.");
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["UserDeleteMessage"] = $"Nutzer {user.FullName} gelöscht";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error deleting user: {error.Description}");
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToAction("Index");
        }

        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Lock the user indefinitely
                await _userManager.UpdateAsync(user);
                TempData["Message"] = $"User {user.FullName} wurde gesperrt.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.LockoutEnd = null; // Unlock the user
                await _userManager.UpdateAsync(user);
                TempData["Message"] = $"User {user.FullName} wurde entsperrt.";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditUserDetails(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return NotFound();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            var allRolesList = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var userRolesList = await _userManager.GetRolesAsync(user);

            var model = new UserDetailsViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                OriginalEmail = user.Email,
                Roles = allRolesList,
                SelectedRoles = userRolesList.ToList()
            };

            ViewBag.RolesSelectList = HtmlHelpers.GetRolesSelectList(allRolesList, userRolesList);

            return View("~/Views/Administration/EditUserDetails.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserDetails(UserDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.OriginalEmail);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;

                var userRolesList = await _userManager.GetRolesAsync(user);
                var rolesToAdd = model.SelectedRoles.Except(userRolesList).ToList();
                var rolesToRemove = userRolesList.Except(model.SelectedRoles).ToList();

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    if (rolesToAdd.Any())
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd);
                    }

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    }

                    TempData["UserEditMessage"] = $"User {user.FullName} erfolgreich editiert.";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var allRolesList = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.RolesSelectList = HtmlHelpers.GetRolesSelectList(
                allRolesList,
                model.SelectedRoles
            );
            model.Roles = allRolesList; // Re-populate roles in case of an error
            return View("~/Views/Administration/EditUserDetails.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUserPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return NotFound();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel { Email = user.Email };

            return View("~/Views/Administration/ChangeUserPassword.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserPassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound();
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    model.Password
                );
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("~/Views/Administration/ChangeUserPassword.cshtml", model);
                }

                TempData["UserEditMessage"] =
                    $"Passwort für User: {user.FullName} wurde erfolgreich geändert.";
                return RedirectToAction("Index");
            }

            return View("~/Views/Administration/ChangeUserPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.RoleName))
            {
                var roleName = model.RoleName.Trim();

                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    return Json(new { success = false, error = "Role existiert bereits." });
                }

                ApplicationRole role = new ApplicationRole { Name = roleName };

                IdentityResult result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role {roleName} created.");
                    return Json(new { success = true });
                }
                else
                {
                    _logger.LogError($"Error creating role: {result.Errors}");
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }

            // Debugging: Log the model state errors
            var errorMessages = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            var errorDetails = string.Join(", ", errorMessages);
            return Json(new { success = false, error = $"Invalid role name: {errorDetails}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(RoleViewModel model)
        {
            if (string.IsNullOrEmpty(model.RoleName))
            {
                return Json(new { success = false, error = "Role name is required." });
            }

            if (model.RoleName == "Administrator")
            {
                return Json(new { success = false, error = "The Admin role cannot be deleted." });
            }

            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role != null)
            {
                IdentityResult result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }

            return Json(new { success = false, error = "Role not found." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string oldRoleName, string newRoleName)
        {
            if (string.IsNullOrEmpty(oldRoleName) || string.IsNullOrEmpty(newRoleName))
            {
                return Json(
                    new { success = false, error = "Both old and new role names are required." }
                );
            }

            var role = await _roleManager.FindByNameAsync(oldRoleName);
            if (role != null)
            {
                role.Name = newRoleName;
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }

            return Json(new { success = false, error = "Role not found." });
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Json(roles);
        }
    }
}
