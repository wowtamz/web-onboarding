using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
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
            if (processId > 0)
            {
                ProcessTemplate processTemplate =
                    await _processTemplateContainer.GetProcessTemplateByIdAsync(processId);
                ViewData["processRoles"] = processTemplate.RolesWithAccess;
            }
            else
            {
                ViewData["processRoles"] = _roleManager.Roles.ToList();
            }
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
                    assignedRole,
                    TempData["startProcessViewModel"] == null || TempData["editProcessViewModel"] == null ? 0 : (int) model.processId
                );

                _logger.LogInformation(
                    "Successfully created a new assignment template with title: {Title}",
                    model.Title
                );
                
                // Author: Tamas Varadi
                // Begin

                bool redirectFromProcessController = TempData["startProcessViewModel"] != null ||
                                                     TempData["editProcessViewModel"] != null;
                
                if (redirectFromProcessController)
                {
                    at.ForContractsList.ForEach(c => c.AssignmentsTemplates = null);
                    at.ForDepartmentsList.ForEach(d => d.AssignmentsTemplates = null);
                    
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

                        Process activeProcess = await _context.Processes.FindAsync(editProcessViewModel.Id);
                        ApplicationUser? assignee = await _userManager.FindByIdAsync(editProcessViewModel.Supervisor.Id);

                        if (at.AssigneeType == AssigneeType.WORKER_OF_REF)
                        {
                            assignee = await _userManager.FindByIdAsync(editProcessViewModel.WorkerOfReference.Id);
                        }

                        Assignment newAssignment = at.ToAssignment(assignee);
                        _context.Assignments.Add(newAssignment);
                        
                        activeProcess.Assignments.Add(newAssignment);

                        await _context.SaveChangesAsync();

                        return RedirectToAction("Edit", "Process", new { id = editProcessViewModel.Id });

                    }
                }
                
                // End

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
            ViewData["roles"] = _roleManager.Roles.ToList();
            ViewData["dueIns"] = _context.DueTimes.ToList();
            ViewData["departments"] = _context.Departments.ToList();
            ViewData["contracts"] = _context.Contracts.ToList();
            
            AssignmentTemplate assignmentTemplate =
                _assignmentTemplateContainer.GetAssignmentTemplate(id);
            /*
            ProcessTemplate processTemplate= await _processTemplateContainer.GetProcessTemplateByIdAsync((int)assignmentTemplate.ProcessTemplateId);
            ViewData["processRoles"] = processTemplate.RolesWithAccess; 
            */
            
            // Überprüfen ob man das AssignmentTemplate einer ProcessTemplate oder Process bearbeitet
            if (assignmentTemplate.ProcessTemplateId > 0)
            {
                ProcessTemplate processTemplate =
                    await _processTemplateContainer.GetProcessTemplateByIdAsync((int)assignmentTemplate.ProcessTemplateId);
                ViewData["processRoles"] = processTemplate.RolesWithAccess;
            }
            else
            {
                ViewData["processRoles"] = _roleManager.Roles.ToList();
            }
            
            if (assignmentTemplate != null)
            {
                CreateEditAssignmentTemplateViewModel createEditAssignmentTemplateVM =
                    new CreateEditAssignmentTemplateViewModel(assignmentTemplate, assignmentTemplate.ProcessTemplateId);
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
                
                // Author: Tamas Varadi
                // Begin
                if (model.processId == 0 || model.processId == null)
                {
                    AssignmentTemplate at = _assignmentTemplateContainer.GetAssignmentTemplate(model.Id);
                    
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

                // End
                
                return RedirectToAction("Edit", "ProcessTemplate", new { id = model.processId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing assignment template.");
                return RedirectToAction("Edit", model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            AssignmentTemplate? assignmentTemplate =
                _assignmentTemplateContainer.GetAssignmentTemplate(id);
            ProcessTemplate processTemplate = await _processTemplateContainer.GetProcessTemplateByIdAsync((int)assignmentTemplate.ProcessTemplateId);
            foreach(var role in processTemplate.RolesWithAccess){
                if(User.IsInRole(role.Name)|| User.IsInRole("Administrator")){
                _assignmentTemplateContainer.DeleteAssignmentTemplate(id);
                break;
                }
            }
            return RedirectToAction("Detail", "ProcessTemplate", new { id = assignmentTemplate.ProcessTemplateId });
        }
    }
}
