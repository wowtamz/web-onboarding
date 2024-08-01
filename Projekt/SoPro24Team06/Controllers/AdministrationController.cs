using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using SoPro24Team06.Data;

//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.Controllers
{
    /// <summary>
    /// Controller for the Administration page
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        private readonly ILogger<AdministrationController> _logger;

        private readonly ApplicationDbContext _context;

        public AdministrationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AdministrationController> logger,
            ApplicationDbContext context
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;

        }

        /// <summary>
        /// Index page for the Administration page
        /// </summary>
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

        /// <summary>
        /// Create User page for the Administration page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var model = new CreateUserViewModel { Roles = roles };
            return View("~/Views/Administration/CreateUser.cshtml", model);
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="model"> The Inputmodel for the User </param>
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

        /// <summary>
        /// Gets the roles of a user
        /// </summary>
        /// <param name="user"> The User to get the roles from </param>
        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

        /// <summary>
        /// Locks a user
        /// </summary>
        /// <param name="userId"> Id of the User to be unlocked </param>
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

        /// <summary>
        /// Unlocks a user
        /// </summary>
        /// <param name="userId"> Id of the User to be unlocked </param>
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

        /// <summary>
        /// Edits the details of a user, including roles, name and email
        /// </summary>
        /// <param name="email"> Email of the User to be edited </param>
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

        /// <summary>
        /// Edits the details of a user, including roles, name and email
        /// </summary>
        /// <param name="model"> The Model of the user to be edited </param>
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

        /// <summary>
        /// Checks if the email is valid and returns the user details
        /// Seperate from EditUserDetails
        /// </summary>
        /// <param name="email"> Email of the User whose Password is to be changed </param>
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

        /// <summary>
        /// Changes the password of a user using the Details from the ChangePasswordViewModel
        /// </summary>
        /// <param name="model"> Model including the User Details </param>
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

        /// <summary>
        /// Creates a role, including necessary checks if the role exists and if the name is valid
        /// </summary>
        /// <param name="model"> Roleviewmodel with the name of the role to be edited </param>
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

        /// <summary>
        /// Deletes a role
        /// Checks if the Role is involved in any Assignments, AssignmentTemplates or ProcessTemplates
        /// </summary>
        /// <param name="model"> RoleViewModel with the name of the role to be deleted </param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(RoleViewModel model)
        {
            if (string.IsNullOrEmpty(model.RoleName))
            {
                return Json(new { success = false, error = "Rollenname ist erforderlich." });
            }

            if (model.RoleName == "Administrator")
            {
                return Json(new { success = false, error = "Die Administrator-Rolle kann nicht gelöscht werden." });
            }

            var involvedAssignments = _context.Assignments
                .Where(a => a.AssignedRole.Name == model.RoleName)
                .ToList();

            var involvedAssignmentTemplates = _context.AssignmentTemplates
                .Where(at => at.AssignedRole.Name == model.RoleName)
                .ToList();

            var involvedProcessTemplates = _context.ProcessTemplates
                .Where(pt => pt.RolesWithAccess.Any(r => r.Name == model.RoleName))
                .ToList();

            if (involvedAssignments.Any() || involvedAssignmentTemplates.Any() || involvedProcessTemplates.Any())
            {
                var assignmentNames = involvedAssignments.Any() 
                    ? string.Join(", ", involvedAssignments.Select(a => a.Title)) 
                    : "";

                var assignmentTemplateNames = involvedAssignmentTemplates.Any() 
                    ? string.Join(", ", involvedAssignmentTemplates.Select(at => at.Title)) 
                    : "";

                var processTemplateNames = involvedProcessTemplates.Any() 
                    ? string.Join(", ", involvedProcessTemplates.Select(pt => pt.Title)) 
                    : "";

                var involvementDetails = new List<string>();

                if (!string.IsNullOrEmpty(assignmentNames))
                {
                    involvementDetails.Add($"Aufgaben: {assignmentNames}");
                }

                if (!string.IsNullOrEmpty(assignmentTemplateNames))
                {
                    involvementDetails.Add($"Aufgabenvorlagen: {assignmentTemplateNames}");
                }

                if (!string.IsNullOrEmpty(processTemplateNames))
                {
                    involvementDetails.Add($"Prozesse: {processTemplateNames}");
                }

                var involvementMessage = string.Join("<br>", involvementDetails);
                var errorMessage = $"Die Rolle '{model.RoleName}' kann nicht gelöscht werden, da sie in den folgenden Elementen involviert ist:<br>{involvementMessage}.";

                TempData["RoleDeleteMessage"] = errorMessage;
                return Json(new { success = false, error = errorMessage });
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

            return Json(new { success = false, error = "Rolle nicht gefunden." });
        }


        /// <summary>
        /// Changes the role name
        /// </summary>
        /// <param name="oldRoleName"> Der alte Rollenname, wird genutzt um die entsprechende Rolle zu finden </param>
        /// <param name="newRoleName"> Der neue Rollenname </param>
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

        /// <summary>
        /// Gets all roles from the RoleManager and returns them as a JSON
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Json(roles);
        }
    }
}
