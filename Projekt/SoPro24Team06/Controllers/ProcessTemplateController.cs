//-------------------------
// Author: Kevin Tornquist
//-------------------------

using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    public class ProcessTemplateController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ProcessTemplateContainer _processTemplateContainer;
        private readonly AssignmentTemplateContainer _assignmentTemplateContainer;
        private readonly ApplicationDbContext _modelContext;
        private readonly ILogger<ProcessTemplateController> _logger;

        public ProcessTemplateController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            ILogger<ProcessTemplateController> logger
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _modelContext = context;
            _processTemplateContainer = new ProcessTemplateContainer(context);
            _assignmentTemplateContainer = new AssignmentTemplateContainer(context);
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["isUserAdmin"] = User.IsInRole("Administrator");

            List<ProcessTemplate> processTemplates =
                await _processTemplateContainer.GetProcessListByAccessRights(User.Identity.Name);

            ProcessTemplateListViewModel model = new() { ProcessTemplateList = processTemplates };
            return View("~/Views/ProcessTemplates/Index.cshtml", model);
        }

        [HttpGet("ProcessTemplate/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            ViewData["isUserAdmin"] = User.IsInRole("Administrator");
            try
            {
                ProcessTemplate processTemplate =
                    await _processTemplateContainer.GetProcessTemplateByIdAsync(id);

                if (processTemplate == null)
                {
                    return RedirectToAction("Index");
                }

                DetailProcessTemplateViewModel model = new(processTemplate);
                return View("~/Views/ProcessTemplates/Detail.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching process template with ID: {Id}", id);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Displays the create view for a new ProcessTemplate.
        /// </summary>
        /// <returns>The create view.</returns>
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            ProcessTemplateViewModel model = new();
            await AddModelsToViewData();

            return View("~/Views/ProcessTemplates/Create.cshtml", model);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("ProcessTemplate/Create")]
        public async Task<IActionResult> Create([FromForm] ProcessTemplateViewModel model)
        {
            await AddModelsToViewData();

            if (!ModelState.IsValid)
            {
                LogModelErrors(model);
                return RedirectToAction("Create");
            }

            try
            {
                /** ----- Start: Fetch objects by primitive values ----- */
                var roles = await _roleManager
                    .Roles.Where(role => model.RolesWithAccess.Contains(role.Name))
                    .ToListAsync();

                var assignments = await _modelContext
                    .AssignmentTemplates.Where(at =>
                        model.SelectedAssignmentTemplateIds.Contains(at.Id)
                    )
                    .ToListAsync();

                var contract = await _modelContext.Contracts.FindAsync(model.ContractOfRefWorkerId);
                if (contract == null)
                {
                    _logger.LogWarning(
                        "Failed to create process template due to invalid contract ID: {ContractId}",
                        model.ContractOfRefWorkerId
                    );
                    return RedirectToAction("Create");
                }

                var department = await _modelContext.Departments.FindAsync(
                    model.DepartmentOfRefWorkerId
                );
                if (department == null)
                {
                    _logger.LogWarning(
                        "Failed to create process template due to invalid department ID: {DepartmentId}",
                        model.DepartmentOfRefWorkerId
                    );
                    return RedirectToAction("Create");
                }

                /** ----- End: Fetch objects by primitive values ----- */

                ProcessTemplate processTemplate = new ProcessTemplate
                {
                    Title = model.Title,
                    Description = model.Description,
                    ContractOfRefWorker = contract,
                    DepartmentOfRefWorker = department,
                    RolesWithAccess = roles,
                    AssignmentTemplates = assignments
                };

                await _processTemplateContainer.AddProcessTemplateAsync(processTemplate);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating process template.");
                return RedirectToAction("Create");
            }
        }

        public async Task<IActionResult> RedirectAssignmentTemplateCreate(
            [FromForm] ProcessTemplateViewModel model
        )
        {
            await AddModelsToViewData();

            if (!ModelState.IsValid)
            {
                LogModelErrors(model);
                if (model.Id != null)
                {
                    return RedirectToAction("Edit", model);
                }
                else
                {
                    return RedirectToAction("Create");
                }
            }

            try
            {
                /** ----- Start: Fetch objects by primitive values ----- */
                var roles = await _roleManager
                    .Roles.Where(role => model.RolesWithAccess.Contains(role.Name))
                    .ToListAsync();

                var assignments = await _modelContext
                    .AssignmentTemplates.Where(at =>
                        model.SelectedAssignmentTemplateIds.Contains(at.Id)
                    )
                    .ToListAsync();

                var contract = await _modelContext.Contracts.FindAsync(model.ContractOfRefWorkerId);
                if (contract == null)
                {
                    _logger.LogWarning(
                        "Failed due to invalid contract ID: {ContractId}",
                        model.ContractOfRefWorkerId
                    );
                    if (model.Id != null)
                    {
                        return RedirectToAction("Edit", model);
                    }
                    else
                    {
                        return RedirectToAction("Create");
                    }
                }

                var department = await _modelContext.Departments.FindAsync(
                    model.DepartmentOfRefWorkerId
                );
                if (department == null)
                {
                    _logger.LogWarning(
                        "Failed due to invalid department ID: {DepartmentId}",
                        model.DepartmentOfRefWorkerId
                    );

                    if (model.Id != null)
                    {
                        return RedirectToAction("Edit", model);
                    }
                    else
                    {
                        return RedirectToAction("Create");
                    }
                }

                /** ----- End: Fetch objects by primitive values ----- */


                if (model.Id != null)
                {
                    ProcessTemplate processTemplate = new ProcessTemplate
                    {
                        Id = (int)model.Id,
                        Title = model.Title,
                        Description = model.Description,
                        ContractOfRefWorker = contract,
                        DepartmentOfRefWorker = department,
                        RolesWithAccess = roles,
                        AssignmentTemplates = assignments
                    };
                    await _processTemplateContainer.UpdateProcessTemplateAsync(processTemplate);

                    return RedirectToAction(
                        "CreateStart",
                        "AssignmentTemplate",
                        new { processId = model.Id }
                    );
                }
                else
                {
                    ProcessTemplate processTemplate = new ProcessTemplate
                    {
                        Title = model.Title,
                        Description = model.Description,
                        ContractOfRefWorker = contract,
                        DepartmentOfRefWorker = department,
                        RolesWithAccess = roles,
                        AssignmentTemplates = assignments
                    };

                    var t = await _processTemplateContainer.AddProcessTemplateAsync(
                        processTemplate
                    );

                    return RedirectToAction(
                        "CreateStart",
                        "AssignmentTemplate",
                        new { processId = t.Id }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating process template.");
                return RedirectToAction("Create");
            }
        }

        [HttpGet("ProcessTemplate/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            ProcessTemplate processTemplate = _processTemplateContainer
                .GetProcessTemplateByIdAsync(id)
                .Result;

            // --- Start: Get the User and their Roles ---
            var user = _modelContext
                .Users.Where(u => u.UserName == User.Identity.Name)
                .FirstOrDefault();

            var userRoles = await _modelContext
                .UserRoles.AsNoTracking()
                .Where(ur => ur.UserId == user.Id)
                .Select(x => x.RoleId)
                .ToListAsync();

            var roles = await _modelContext
                .Roles.AsNoTracking()
                .Where(r => userRoles.Contains(r.Id))
                .ToListAsync();

            if (!roles.Select(x => x.Name).Contains("Administrator"))
            {
                try
                {
                    foreach (var role in roles)
                    {
                        _logger.LogInformation(
                            "User {UserName} has role {Role}",
                            User.Identity.Name,
                            role.Name
                        );
                    }

                    foreach (var role in processTemplate.RolesWithAccess)
                    {
                        _logger.LogInformation(
                            "Process template with ID {TemplateId} has role {Role}",
                            processTemplate.Id,
                            role.Name
                        );
                    }

                    // if the process template roles with access do not contain any of the user roles, redirect to index
                    if (
                        !processTemplate.RolesWithAccess.Any(r =>
                            roles.Select(r => r.Name).Contains(r.Name)
                        )
                    )
                    {
                        _logger.LogWarning(
                            "User {UserName} is not authorized to edit process template with ID {TemplateId}",
                            User.Identity.Name,
                            processTemplate.Id
                        );
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching user roles.");
                    return RedirectToAction("Index");
                }
            }
            // --- End: Get the User and their Roles ---

            ProcessTemplateViewModel model =
                new()
                {
                    Id = processTemplate.Id,
                    Title = processTemplate.Title,
                    Description = processTemplate.Description,
                    ContractOfRefWorkerId = processTemplate.ContractOfRefWorker.Id,
                    DepartmentOfRefWorkerId = processTemplate.DepartmentOfRefWorker.Id,
                    RolesWithAccess = processTemplate.RolesWithAccess.Select(r => r.Name).ToList(),
                    SelectedAssignmentTemplateIds = processTemplate
                        .AssignmentTemplates.Select(a => a.Id)
                        .ToList()
                };

            await AddModelsToViewData();

            return View("~/Views/ProcessTemplates/Edit.cshtml", model);
        }

        [HttpPost("ProcessTemplate/Edit/{id:int}")]
        public async Task<IActionResult> Edit([FromForm] ProcessTemplateViewModel model)
        {
            _logger.LogInformation("Editing process template with {template}", model.ToJson());
            if (!ModelState.IsValid)
            {
                LogModelErrors(model);
                return RedirectToAction("Edit", model);
            }
            var pt = await _modelContext
                .ProcessTemplates.Include(pt => pt.RolesWithAccess)
                .FirstOrDefaultAsync(pt => pt.Id == model.Id);

            var user = _modelContext
                .Users.Where(u => u.UserName == User.Identity.Name)
                .FirstOrDefault();

            var userRoles = await _modelContext
                .UserRoles.AsNoTracking()
                .Where(ur => ur.UserId == user.Id)
                .Select(x => x.RoleId)
                .ToListAsync();

            var rolesUser = await _modelContext
                .Roles.AsNoTracking()
                .Where(r => userRoles.Contains(r.Id))
                .ToListAsync();

            // --- Start: Get the User and their Roles ---
            if (!rolesUser.Select(x => x.Name).Contains("Administrator"))
            {
                try
                {
                    foreach (var role in rolesUser)
                    {
                        _logger.LogInformation(
                            "User {UserName} has role {Role}",
                            User.Identity.Name,
                            role.Name
                        );
                    }

                    foreach (var role in pt.RolesWithAccess)
                    {
                        _logger.LogInformation(
                            "Process template with ID {TemplateId} has role {Role}",
                            pt.Id,
                            role.Name
                        );
                    }

                    // if the process template roles with access do not contain any of the user roles, redirect to index
                    if (
                        !pt.RolesWithAccess.Any(r => rolesUser.Select(r => r.Name).Contains(r.Name))
                    )
                    {
                        _logger.LogWarning(
                            "User {UserName} is not authorized to edit process template with ID {TemplateId}",
                            User.Identity.Name,
                            model.Id
                        );
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching user roles.");
                    return RedirectToAction("Index");
                }
            }
            // --- End: Get the User and their Roles ---

            try
            {
                if (model.Id == 0)
                {
                    _logger.LogWarning("Invalid process template ID: {TemplateId}", model.Id);
                    return RedirectToAction("Index");
                }

                /** ----- Start: Fetch objects by primitive values ----- */
                var roles = await _roleManager
                    .Roles.Where(role => model.RolesWithAccess.Contains(role.Name))
                    .ToListAsync();

                var assignments = await _modelContext
                    .AssignmentTemplates.Where(at =>
                        model.SelectedAssignmentTemplateIds.Contains(at.Id)
                    )
                    .ToListAsync();

                var contract = await _modelContext.Contracts.FindAsync(model.ContractOfRefWorkerId);
                if (contract == null)
                {
                    _logger.LogError(
                        "Failed to create process template due to invalid contract ID: {ContractId}",
                        model.ContractOfRefWorkerId
                    );
                    return RedirectToAction("Edit", model);
                }

                var department = await _modelContext.Departments.FindAsync(
                    model.DepartmentOfRefWorkerId
                );
                if (department == null)
                {
                    _logger.LogError(
                        "Failed to create process template due to invalid department ID: {DepartmentId}",
                        model.DepartmentOfRefWorkerId
                    );
                    return RedirectToAction("Edit", model);
                }
                /** ----- End: Fetch objects by primitive values ----- */

                ProcessTemplate templateToEdit = new ProcessTemplate
                {
                    Id = (int)model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    ContractOfRefWorker = contract,
                    DepartmentOfRefWorker = department,
                    RolesWithAccess = roles,
                    AssignmentTemplates = assignments
                };

                await _processTemplateContainer.UpdateProcessTemplateAsync(templateToEdit);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to edit process template with ID: {TemplateId}",
                    model.Id
                );
                RedirectToAction("Edit", new { id = model.Id });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("ProcessTemplate/Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _processTemplateContainer.DeleteProcessTemplateAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting process template.");
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Adds necessary data to ViewData for views to use.
        /// </summary>
        public async Task AddModelsToViewData()
        {
            List<ApplicationUser> users = _userManager.Users.ToList();
            List<ProcessTemplate> processTemplates =
                await _processTemplateContainer.GetProcessTemplatesAsync();
            List<AssignmentTemplate> assignmentTemplates =
                await _assignmentTemplateContainer.GetAllAssignmentTemplates();

            // Replace with Containers;
            List<Assignment> assignments = _modelContext.Assignments.ToList();
            List<Contract> contracts = _modelContext.Contracts.ToList();
            List<Department> departments = _modelContext.Departments.ToList();
            List<ApplicationRole> roles = _roleManager.Roles.ToList();

            ViewData["Users"] = users;
            ViewData["ProcessTemplates"] = processTemplates;
            ViewData["Assignments"] = assignments;
            ViewData["AssignmentTemplates"] = assignmentTemplates;
            ViewData["Contracts"] = contracts;
            ViewData["Departments"] = departments;
            ViewData["Roles"] = roles;
        }

        /// <summary>
        /// Checks if the current user is authorized based on the specified role.
        /// </summary>
        /// <param name="role">The role to check authorization for.</param>
        /// <returns>True if the user is authorized or administrator, otherwise false.</returns>
        public bool IsAuthorized(string role)
        {
            if (User.IsInRole("Administrator"))
            {
                return true;
            }
            else
            {
                return User.IsInRole(role);
            }
        }

        /// <summary>
        /// Logs model errors for debugging purposes.
        /// </summary>
        /// <param name="model">The ProcessTemplate view model.</param>
        private void LogModelErrors(ProcessTemplateViewModel model)
        {
            _logger.LogError("Model: {Model}", model);
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    _logger.LogError("Error: {Error}", error.ErrorMessage);
                }
            }
        }
    }
}
