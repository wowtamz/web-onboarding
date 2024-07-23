//-------------------------
// Author: Kevin Tornquist
//-------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    [Authorize]
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
            List<ProcessTemplate> processTemplates =
                await _processTemplateContainer.GetProcessTemplatesAsync();

            ProcessTemplateListViewModel model = new() { ProcessTemplateList = processTemplates };
            return View("~/Views/ProcessTemplates/Index.cshtml", model);
        }

        [HttpGet("ProcessTemplate/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
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

        public async Task<IActionResult> Create()
        {
            ProcessTemplateViewModel model = new();
            await AddModelsToViewData();

            return View("~/Views/ProcessTemplates/Create.cshtml", model);
        }

        [HttpPost("ProcessTemplate/Create")]
        public async Task<IActionResult> Create([FromForm] ProcessTemplateViewModel model)
        {
            await AddModelsToViewData();

            if (!ModelState.IsValid)
            {
                _logger.LogError("Model: {Model}", model.ToJson());
                foreach (var modelState in ModelState.Values)
                {
                    _logger.LogError("Model state: {ModelState}", modelState.ToJson());
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("Error: {Error}", error.ErrorMessage);
                    }
                }
                _logger.LogWarning("Create operation failed due to invalid model state.");
                return RedirectToAction("Create");
            }

            try
            {
                /** ----- Start: Fetch objects by primitve values ----- */
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

                /** ----- End: Fetch objects by primitve values ----- */

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
                _logger.LogError("Model: {Model}", model.ToJson());
                foreach (var modelState in ModelState.Values)
                {
                    _logger.LogError("Model state: {ModelState}", modelState.ToJson());
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("Error: {Error}", error.ErrorMessage);
                    }
                }
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
                /** ----- Start: Fetch objects by primitve values ----- */
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

                /** ----- End: Fetch objects by primitve values ----- */


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
                foreach (var modelState in ModelState.Values)
                {
                    _logger.LogError("Model state: {ModelState}", modelState);
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("Error: {Error}", error.ErrorMessage);
                    }
                }
                _logger.LogWarning(
                    "Edit operation failed due to invalid model state for template ID: {TemplateId}",
                    model.Id
                );
                return RedirectToAction("Edit", model);
            }

            try
            {
                if (model.Id == 0)
                {
                    _logger.LogWarning("Invalid process template ID: {TemplateId}", model.Id);
                    return RedirectToAction("Index");
                }

                /** ----- Start: Fetch objects by primitve values ----- */
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
                /** ----- End: Fetch objects by primitve values ----- */

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

        public async Task AddModelsToViewData()
        {
            List<ApplicationUser> users = _userManager.Users.ToList();
            List<ProcessTemplate> processTemplates =
                await _processTemplateContainer.GetProcessTemplatesAsync();
            List<AssignmentTemplate> assignmentTemplates =
                _assignmentTemplateContainer.GetAllAssignmentTemplates();

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
    }
}
