using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoPro24Team06.Containers;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    [Authorize]
    public class ProcessTemplateController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ProcessTemplateContainer _processTemplateContainer = new();

        private readonly AssignmentTemplateContainer _assignmentTemplateContainer = new();

        private readonly ILogger<ProcessTemplateController> _logger;

        public ProcessTemplateController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<ProcessTemplateController> logger
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            List<ProcessTemplate> processTemplates =
                await _processTemplateContainer.GetProcessTemplatesAsync();
            _logger.LogInformation("ProcessTemplates: " + processTemplates.ToArray());

            ProcessTemplateListViewModel model = new() { ProcessTemplateList = processTemplates };
            return View("~/Views/ProcessTemplates/Index.cshtml", model);
        }

        public IActionResult Create()
        {
            ProcessTemplateViewModel model = new() { Roles = _roleManager.Roles.ToList() };

            return View("~/Views/ProcessTemplates/Create.cshtml", model);
        }

        [HttpPost("ProcessTemplate/Create")]
        public async Task<IActionResult> Create([FromForm] ProcessTemplateViewModel model)
        {
            _logger.LogInformation("Starting the creation of a new process template.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create operation failed due to invalid model state.");
                return RedirectToAction("Create");
            }

            try
            {
                ProcessTemplate processTemplate =
                    new()
                    {
                        Title = model.Title,
                        Description = model.Description,
                        AssignmentTemplates = model.AssignmentTemplates,
                        ContractOfRefWorker = model.ContractOfRefWorker,
                        DepartmentOfRefWorker = model.DepartmentOfRefWorker,
                        RolesWithAccess = model.RolesWithAccess
                    };

                await _processTemplateContainer.AddProcessTemplateAsync(processTemplate);

                _logger.LogInformation(
                    "Successfully created a new process template with title: {Title}",
                    model.Title
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating process template.");
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(ProcessTemplate processTemplate)
        {
            ProcessTemplateViewModel model = new(processTemplate);
            model.Roles = _roleManager.Roles.ToList();
            return View("~/Views/ProcessTemplates/Edit.cshtml", model);
        }

        [HttpGet("ProcessTemplate/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            ProcessTemplate processTemplate = _processTemplateContainer
                .GetProcessTemplateByIdAsync(id)
                .Result;

            if (processTemplate == null)
            {
                return View("Index");
            }

            ProcessTemplateViewModel model = new(processTemplate);

            return View("~/Views/ProcessTemplates/Edit.cshtml", model);
        }

        [HttpPost("ProcessTemplate/Edit/{id}")]
        public async Task<IActionResult> Edit([FromForm] ProcessTemplateViewModel model)
        {
            _logger.LogInformation(
                "Attempting to edit process template with ID: {TemplateId}",
                model.Id
            );

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Edit operation failed due to invalid model state for template ID: {TemplateId}",
                    model.Id
                );
                return View("~/Views/ProcessTemplates/Edit.cshtml", model);
            }

            try
            {
                await _processTemplateContainer.UpdateProcessTemplateAsync(
                    model.Id.Value,
                    model.Title,
                    model.Description,
                    model.AssignmentTemplates
                );
                _logger.LogInformation(
                    "Process template with ID: {TemplateId} has been updated successfully.",
                    model.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to edit process template with ID: {TemplateId}",
                    model.Id
                );
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete()
        {
            return View("~/Views/ProcessTemplates/Index.cshtml");
        }

        [HttpPost]
        public IActionResult AddAssignment(int id)
        {
            var template = _assignmentTemplateContainer.GetAssignmentTemplate(id);
            // Optionally, add to session or DB here
            return Json(new { success = true, html = GenerateHtmlForAssignment(template) });
        }

        private string GenerateHtmlForAssignment(AssignmentTemplate template)
        {
            return $"<tr><td>{template.Title}</td><td><button onclick='removeAssignment(this, {template.Id})'>Remove</button></td></tr>";
        }
    }
}
