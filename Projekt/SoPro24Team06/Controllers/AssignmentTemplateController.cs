using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    [Authorize]
    public class AssignmentTemplateController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ProcessTemplateContainer _processTemplateContainer;
        private readonly ILogger<AssignmentTemplateController> _logger;
        private readonly AssignmentTemplateContainer _assignmentTemplateContainer;
        private readonly DueTimeContainer _dueTimeContainer;

        public AssignmentTemplateController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AssignmentTemplateController> logger
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _processTemplateContainer = new ProcessTemplateContainer(context);
            _assignmentTemplateContainer = new AssignmentTemplateContainer(context);
            _dueTimeContainer = new DueTimeContainer(context);
            _logger = logger;
        }

        [HttpGet("AssignmentTemplate/Create/{processId}")]
        public async Task<IActionResult> CreateStart(int processId)
        {
            ViewData["roles"] = _roleManager.Roles.ToList();
            ViewData["dueIns"] = _context.DueTimes.ToList();
            ViewData["departments"] = _context.Departments.ToList();
            ViewData["contracts"] = _context.Contracts.ToList();
            CreateEditAssignmentTemplateViewModel assignmentTemplateVM =
                new CreateEditAssignmentTemplateViewModel(processId);
            return View("~/Views/Assignments/Create.cshtml", assignmentTemplateVM);
        }

        [HttpPost("AssignmentTemplate/Create")]
        public async Task<IActionResult> Create(
            [FromForm] CreateEditAssignmentTemplateViewModel model
        )
        {
            _logger.LogInformation("Starting the creation of a new assignment template.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create operation failed due to invalid model state.");
                return RedirectToAction("Create");
            }

            List<Department> departmentsList = new List<Department>();
            if (model.ForDepartmentsList != null)
            {
                foreach (var d in model.ForDepartmentsList)
                {
                    Department department = _context.Departments.FirstOrDefault(department =>
                        department.Name == d
                    );
                    departmentsList.Add(department);
                }
            }

            List<Contract> contractsList = new List<Contract>();
            if (model.ForContractsList != null)
            {
                foreach (var c in model.ForContractsList)
                {
                    Contract contract = _context.Contracts.FirstOrDefault(contract =>
                        contract.Label == c
                    );
                    contractsList.Add(contract);
                }
            }

            ApplicationRole assignedRole = null;
            AssigneeType assigneeType;
            if (Enum.TryParse(model.AssigneeType, out assigneeType))
            {
                if (assigneeType == AssigneeType.ROLES)
                {
                    assignedRole = await _roleManager.FindByNameAsync(model.AssignedRole);
                }
            }

            string dueTime;
            DueTime dueIn = _context.DueTimes.FirstOrDefault(dueTime =>
                dueTime.Label == model.DueIn
            );
            if (model.DueIn == "Benutzerdefiniert")
            {
                if (model.VorNach == "Vor:")
                {
                    dueTime =
                        $"{model.Days} Tage {model.Weeks} Wochen {model.Months} Monate vor Start";
                    dueIn = new DueTime(
                        dueTime,
                        model.Days * -1,
                        model.Weeks * -1,
                        model.Months * -1
                    );
                    _dueTimeContainer.AddDueTime(dueIn);
                }
                else
                {
                    dueTime =
                        $"{model.Days} Tage {model.Weeks} Wochen {model.Months} Monate nach Arbeitsbeginn";
                    dueIn = new DueTime(dueTime, model.Days, model.Weeks, model.Months);
                    _dueTimeContainer.AddDueTime(dueIn);
                }
            }

            try
            {
                var at = _assignmentTemplateContainer.AddAssignmentTemplate(
                    model.Title ?? "",
                    model.Instructions,
                    dueIn,
                    departmentsList,
                    contractsList,
                    assigneeType,
                    assignedRole
                );

                _logger.LogInformation(
                    "Successfully created a new assignment template with title: {Title}",
                    model.Title
                );

                var processTemplate = await _processTemplateContainer.GetProcessTemplateByIdAsync(
                    (int)model.processId
                );
                processTemplate.AssignmentTemplates.Add(at);
                await _processTemplateContainer.UpdateProcessTemplateAsync(processTemplate);

                return RedirectToAction("Edit", "ProcessTemplate", new { id = model.processId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignment template.");
                return RedirectToAction("Create");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            string queryProcessId = HttpContext.Request.Query["processId"].ToString();
            int processId = Int32.Parse(queryProcessId);
            ViewData["roles"] = _roleManager.Roles.ToList();
            ViewData["dueIns"] = _context.DueTimes.ToList();
            ViewData["departments"] = _context.Departments.ToList();
            ViewData["contracts"] = _context.Contracts.ToList();
            AssignmentTemplate assignmentTemplate =
                _assignmentTemplateContainer.GetAssignmentTemplate(id);
            if (assignmentTemplate != null)
            {
                CreateEditAssignmentTemplateViewModel createEditAssignmentTemplateVM =
                    new CreateEditAssignmentTemplateViewModel(assignmentTemplate, processId);
                return View("~/Views/Assignments/Edit.cshtml", createEditAssignmentTemplateVM);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            [FromForm] CreateEditAssignmentTemplateViewModel model
        )
        {
            _logger.LogInformation("Starting the editing of an assignment template.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Edit operation failed due to invalid model state.");
                return RedirectToAction("Edit", model.Id);
            }

            List<Department> departmentsList = new List<Department>();
            if (model.ForDepartmentsList != null)
            {
                foreach (var d in model.ForDepartmentsList)
                {
                    Department department = _context.Departments.FirstOrDefault(department =>
                        department.Name == d
                    );
                    departmentsList.Add(department);
                }
            }

            List<Contract> contractsList = new List<Contract>();
            if (model.ForContractsList != null)
            {
                foreach (var c in model.ForContractsList)
                {
                    Contract contract = _context.Contracts.FirstOrDefault(contract =>
                        contract.Label == c
                    );
                    contractsList.Add(contract);
                }
            }

            ApplicationRole assignedRole = null;
            AssigneeType assigneeType;
            if (Enum.TryParse(model.AssigneeType, out assigneeType))
            {
                if (assigneeType == AssigneeType.ROLES)
                {
                    assignedRole = await _roleManager.FindByNameAsync(model.AssignedRole);
                }
            }

            string dueTime;
            DueTime dueIn = _context.DueTimes.FirstOrDefault(dueTime =>
                dueTime.Label == model.DueIn
            );
            if (model.DueIn == "Benutzerdefiniert")
            {
                if (model.VorNach == "Vor:")
                {
                    dueTime =
                        $"{model.Days} Tage {model.Weeks} Wochen {model.Months} Monate vor Start";
                    dueIn = new DueTime(
                        dueTime,
                        model.Days * -1,
                        model.Weeks * -1,
                        model.Months * -1
                    );
                    _context.DueTimes.Add(dueIn);
                    _context.SaveChanges();
                }
                else
                {
                    dueTime =
                        $"{model.Days} Tage {model.Weeks} Wochen {model.Months} Monate nach Arbeitsbeginn";
                    dueIn = new DueTime(dueTime, model.Days, model.Weeks, model.Months);
                    _context.DueTimes.Add(dueIn);
                    _context.SaveChanges();
                }
            }

            try
            {
                _assignmentTemplateContainer.EditAssignmentTemplates(
                    model.Id,
                    model.Title ?? "",
                    model.Instructions,
                    dueIn,
                    departmentsList,
                    contractsList,
                    assigneeType,
                    assignedRole
                );

                _logger.LogInformation(
                    "Successfully edited the assignment template with title: {Title}",
                    model.Title
                );
                return RedirectToAction("Detail", "ProcessTemplate", new { id = model.processId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing assignment template.");
                return RedirectToAction("Edit", model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            AssignmentTemplate? assignmentTemplate =
                _assignmentTemplateContainer.GetAssignmentTemplate(id);
            _assignmentTemplateContainer.DeleteAssignmentTemplate(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
