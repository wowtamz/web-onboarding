//-------------------------
// Author: Vincent Steiner
//-------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.CodeAnalysis;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Interfaces;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    public class AssignmentTemplateController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ProcessTemplateContainer _processTemplateContainer;
        private readonly ILogger<AssignmentTemplateController> _logger;
        private readonly IAssignmentTemplate _assignmentTemplateContainer;
        private readonly IDueTime _dueTimeContainer;
        private readonly IDepartment _departmentContainer;
        private readonly IContract _contractContainer;

        public AssignmentTemplateController( // Initialisieren des Controllers
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
            _departmentContainer = new DepartmentContainer(context);
            _contractContainer = new ContractContainer(context);
            _logger = logger;
        }

        [HttpGet("AssignmentTemplate/Create/{processId}")] // Bekommt von aus dem ProcessTemplateController eine ProcessTemplateId übergeben, damit man am Ende der Create Funktion das erstellte Assignment Template einem Process Template zuweisen kann
        public async Task<IActionResult> CreateStart(int processId)
        {
            ViewData["roles"] = _roleManager.Roles.ToList();
            ViewData["dueIns"] = _context.DueTimes.ToList();
            ViewData["departments"] = _context.Departments.ToList();
            ViewData["contracts"] = _context.Contracts.ToList();
            CreateEditAssignmentTemplateViewModel assignmentTemplateVM =
                new CreateEditAssignmentTemplateViewModel(processId);
            if (processId > 0)
            {
                ProcessTemplate processTemplate =
                    await _processTemplateContainer.GetProcessTemplateByIdAsync(processId);
                foreach (var role in processTemplate.RolesWithAccess) // Sucht aus dem ProcessTemplate die Rollen raus die Zugriff auf das ProcessTemplate haben. Damit wird im Backend abgesichert wer Zugriff auf die Erstellung der AssignmentTemplates hat
                {
                    if (User.IsInRole(role.Name) || User.IsInRole("Administrator"))
                    {
                        ViewData["processRoles"] = processTemplate.RolesWithAccess;
                        return View("~/Views/Assignments/Create.cshtml", assignmentTemplateVM);
                        break;
                    }
                }
                return NotFound();
            }
            else
            {
                ViewData["processRoles"] = _roleManager.Roles.ToList();
            }
            return View("~/Views/Assignments/Create.cshtml", assignmentTemplateVM);
        }

        [HttpPost("AssignmentTemplate/Create")]
        public async Task<IActionResult> Create( // CreateEditAssignmentTemplateViewModel besteht aus einfachen Datentypen und die werden zu komplexen Datentypen umgewandelt und dann als AssignmentTemplate abgespeichert
            [FromForm] CreateEditAssignmentTemplateViewModel model
        )
        {
            if (await UserHasAccess(model.processId))
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
                        Department department = await _departmentContainer.GetDepartment(d);
                        departmentsList.Add(department);
                    }
                }

                List<Contract> contractsList = new List<Contract>();
                if (model.ForContractsList != null)
                {
                    foreach (var c in model.ForContractsList)
                    {
                        Contract contract = await _contractContainer.GetContract(c);
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

                DueTime dueIn = await _dueTimeContainer.GetDueTime(model.DueIn);
                if (model.DueIn == "Benutzerdefiniert")
                {
                    string dueTime;
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

                AssignmentTemplate at = await _assignmentTemplateContainer.AddAssignmentTemplate(
                    model.Title ?? "",
                    model.Instructions,
                    dueIn,
                    departmentsList,
                    contractsList,
                    assigneeType,
                    assignedRole,
                    TempData["startProcessViewModel"] != null
                    || TempData["editProcessViewModel"] != null
                        ? 0
                        : (int)model.processId
                );

                if (at == null)
                {
                    _logger.LogError("Error creating assignment template.");
                    return RedirectToAction("Create");
                }

                _logger.LogInformation(
                    "Successfully created a new assignment template with title: {Title}",
                    model.Title
                );

                // Author: Tamas Varadi
                // Begin

                bool redirectFromProcessController =
                    TempData["startProcessViewModel"] != null
                    || TempData["editProcessViewModel"] != null;

                if (redirectFromProcessController)
                {
                    at.ForContractsList.ForEach(c => c.AssignmentsTemplates = null);
                    at.ForDepartmentsList.ForEach(d => d.AssignmentsTemplates = null);
                    if (at.AssignedRole != null)
                    {
                        at.AssignedRole.ProcessTemplates = null;
                    }

                    if (TempData["startProcessViewModel"] != null)
                    {
                        string jsonViewModel = TempData["startProcessViewModel"] as string;
                        StartProcessViewModel startProcessViewModel =
                            JsonConvert.DeserializeObject<StartProcessViewModel>(jsonViewModel);

                        startProcessViewModel.AssignmentTemplates.Add(at);

                        jsonViewModel = JsonConvert.SerializeObject(startProcessViewModel);
                        TempData["startProcessViewModel"] = jsonViewModel;

                        return RedirectToAction("Start", "Process");
                    }

                    if (TempData["editProcessViewModel"] != null)
                    {
                        string jsonViewModel = TempData["editProcessViewModel"] as string;
                        EditProcessViewModel editProcessViewModel =
                            JsonConvert.DeserializeObject<EditProcessViewModel>(jsonViewModel);

                        Process activeProcess = await _context.Processes.FindAsync(
                            editProcessViewModel.Id
                        );
                        ApplicationUser? assignee = await _userManager.FindByIdAsync(
                            editProcessViewModel.Supervisor.Id
                        );

                        if (at.AssigneeType == AssigneeType.WORKER_OF_REF)
                        {
                            assignee = await _userManager.FindByIdAsync(
                                editProcessViewModel.WorkerOfReference.Id
                            );
                        }

                        Assignment newAssignment = at.ToAssignment(assignee, activeProcess.DueDate);
                        _context.Assignments.Add(newAssignment);

                        activeProcess.Assignments.Add(newAssignment);

                        await _context.SaveChangesAsync();

                        return RedirectToAction(
                            "Edit",
                            "Process",
                            new { id = editProcessViewModel.Id }
                        );
                    }
                }
            }

            // End

            return RedirectToAction("Edit", "ProcessTemplate", new { id = model.processId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["roles"] = _roleManager.Roles.ToList();
            ViewData["dueIns"] = _context.DueTimes.ToList();
            ViewData["departments"] = _context.Departments.ToList();
            ViewData["contracts"] = _context.Contracts.ToList();

            AssignmentTemplate assignmentTemplate = await
                _assignmentTemplateContainer.GetAssignmentTemplate(id);

            if (assignmentTemplate == null)
            {
                return NotFound();
            }

            // Überprüfen ob man das AssignmentTemplate einer ProcessTemplate oder Process bearbeitet
            if (assignmentTemplate.ProcessTemplateId > 0)
            {
                ProcessTemplate processTemplate =
                    await _processTemplateContainer.GetProcessTemplateByIdAsync(
                        (int)assignmentTemplate.ProcessTemplateId
                    );
                ViewData["processRoles"] = processTemplate.RolesWithAccess;

                foreach (var role in processTemplate.RolesWithAccess)
                {
                    if (User.IsInRole(role.Name) || User.IsInRole("Administrator"))
                    {
                        CreateEditAssignmentTemplateViewModel createEditAssignmentTemplateVM =
                            new CreateEditAssignmentTemplateViewModel(
                                assignmentTemplate,
                                assignmentTemplate.ProcessTemplateId
                            );
                        return View(
                            "~/Views/Assignments/Edit.cshtml",
                            createEditAssignmentTemplateVM
                        );
                    }
                }
            }
            else
            {
                ViewData["processRoles"] = _roleManager.Roles.ToList();
                if (User.IsInRole("Administrator"))
                {
                    CreateEditAssignmentTemplateViewModel createEditAssignmentTemplateVM =
                        new CreateEditAssignmentTemplateViewModel(
                            assignmentTemplate,
                            assignmentTemplate.ProcessTemplateId
                        );
                    return View("~/Views/Assignments/Edit.cshtml", createEditAssignmentTemplateVM);
                }
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit( // CreateEditAssignmentTemplateViewModel besteht aus einfachen Datentypen und die werden zu komplexen Datentypen umgewandelt und dann als editiertes AssignmentTemplate abgespeichert
            [FromForm] CreateEditAssignmentTemplateViewModel model
        )
        {
            if (await UserHasAccess(model.processId))
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
                        Department department = await _departmentContainer.GetDepartment(d);
                        departmentsList.Add(department);
                    }
                }

                List<Contract> contractsList = new List<Contract>();
                if (model.ForContractsList != null)
                {
                    foreach (var c in model.ForContractsList)
                    {
                        Contract contract = await _contractContainer.GetContract(c);
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
                DueTime dueIn = await _dueTimeContainer.GetDueTime(model.DueIn);
                if (model.DueIn == "Benutzerdefiniert")
                {
                    if (model.VorNach == "Vor:" || model.VorNach == "vor Start")
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

                // Author: Tamas Varadi
                // Begin
                if (model.processId == 0 || model.processId == null)
                {
                    AssignmentTemplate at = await _assignmentTemplateContainer.GetAssignmentTemplate(
                        model.Id
                    );
                    
                    at.ForContractsList.ForEach(c => c.AssignmentsTemplates = null);
                    at.ForDepartmentsList.ForEach(d => d.AssignmentsTemplates = null);
                    if (at.AssignedRole != null)
                    {
                        at.AssignedRole.ProcessTemplates = null;
                    }

                    if (TempData["startProcessViewModel"] != null)
                    {
                        string jsonViewModel = TempData["startProcessViewModel"] as string;
                        StartProcessViewModel startProcessViewModel =
                            JsonConvert.DeserializeObject<StartProcessViewModel>(jsonViewModel);

                        startProcessViewModel.AssignmentTemplates.RemoveAll(a => a.Id == model.Id);
                        startProcessViewModel.AssignmentTemplates.Add(at);

                        jsonViewModel = JsonConvert.SerializeObject(startProcessViewModel);
                        TempData["startProcessViewModel"] = jsonViewModel;

                        return RedirectToAction("Start", "Process");
                    }
                }
            }
            // End

            return RedirectToAction("Edit", "ProcessTemplate", new { id = model.processId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id) // Sucht mit der Id ein Assignment Template aus der DB und löscht dieses dann
        {
            AssignmentTemplate? assignmentTemplate = await
                _assignmentTemplateContainer.GetAssignmentTemplate(id);
            ProcessTemplate processTemplate =
                await _processTemplateContainer.GetProcessTemplateByIdAsync(
                    (int)assignmentTemplate.ProcessTemplateId
                );
            foreach (var role in processTemplate.RolesWithAccess) // Sucht aus dem ProcessTemplate die Rollen raus die Zugriff auf das ProcessTemplate haben. Damit wird im Backend abgesichert wer Zugriff auf die Bearbeitung der AssignmentTemplate hat und dieses löschen kann
            {
                if (User.IsInRole(role.Name) || User.IsInRole("Administrator"))
                {
                    _assignmentTemplateContainer.DeleteAssignmentTemplate(id);
                    break;
                }
            }
            return RedirectToAction(
                "Detail",
                "ProcessTemplate",
                new { id = assignmentTemplate.ProcessTemplateId }
            );
        }
        
        // Author: Tamas Varadi
        // Begin
        public async Task<bool> UserHasAccess(int? processTemplateId)
        {
            bool redirectFromProcessController =
                TempData["startProcessViewModel"] != null
                || TempData["editProcessViewModel"] != null;

            bool isSupervisor = false;

            if (redirectFromProcessController || processTemplateId == 0 ||processTemplateId == null)
            {
                if (TempData["startProcessViewModel"] != null)
                {
                    string jsonViewModel = TempData["startProcessViewModel"] as string;
                    StartProcessViewModel startProcessViewModel =
                        JsonConvert.DeserializeObject<StartProcessViewModel>(jsonViewModel);

                    processTemplateId = startProcessViewModel.Template.Id;
                }

                if (TempData["editProcessViewModel"] != null)
                {
                    string jsonViewModel = TempData["editProcessViewModel"] as string;
                    EditProcessViewModel editProcessViewModel =
                        JsonConvert.DeserializeObject<EditProcessViewModel>(jsonViewModel);

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return editProcessViewModel.Supervisor.Id == userId || User.IsInRole("Administrator");
                }
            }
            ProcessTemplate processTemplate =
                await _processTemplateContainer.GetProcessTemplateByIdAsync((int)processTemplateId);

            foreach (var role in
                     processTemplate
                         .RolesWithAccess) // Sucht aus dem ProcessTemplate die Rollen raus die Zugriff auf das ProcessTemplate haben. Damit wird im Backend abgesichert wer Zugriff auf die Erstellung der AssignmentTemplates hat
            {
                if (User.IsInRole(role.Name) || User.IsInRole("Administrator") || isSupervisor)
                {
                    return true;
                }
            }

            return false;
        }
        // End
    }
}
