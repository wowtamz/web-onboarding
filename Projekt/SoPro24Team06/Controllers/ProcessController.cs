//-------------------------
// Author: Tamas Varadi
//-------------------------

using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using ActiveProcess = SoPro24Team06.Models.Process;

namespace SoPro24Team06.Controllers
{
    [Authorize]
    public class ProcessController : Controller
    {
        public ProcessContainer _processContainer;
        public ProcessTemplateContainer _processTemplateContainer;
        public AssignmentTemplateContainer _assignmentTemplateContainer;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ProcessController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _processContainer = new ProcessContainer(context, userManager, roleManager);
            _processTemplateContainer = new ProcessTemplateContainer(context);
            _assignmentTemplateContainer = new AssignmentTemplateContainer(context);
        }

        public async Task<IActionResult> Index()
        {
            string userId = (string)GetCurrentUserId();

            List<ActiveProcess> myProcessList =
                await _processContainer.GetActiveProcessesOfUserAsync(userId);
            List<ActiveProcess> archivedProcessList =
                await _processContainer.GetArchivedProcessesOfUserAsync(userId);

            ViewData["MyProcesses"] = myProcessList;
            ViewData["ArchivedProcesses"] = archivedProcessList;
            ViewData["CanStartProcesses"] = await UserCanStartProcesses();

            if (User.IsInRole("Administrator"))
            {
                List<ActiveProcess> allActiveProcessList =
                    await _processContainer.GetActiveProcessesAsync();
                List<ActiveProcess> allArchivedProcessList =
                    await _processContainer.GetArchivedProcessesAsync();

                ViewData["AllProcesses"] = allActiveProcessList;
                ViewData["AllArchivedProcesses"] = allArchivedProcessList;
            }

            ProcessListViewModel processListViewModel = new ProcessListViewModel(myProcessList);
            return View(processListViewModel);
        }

        // Vorgang starten Seite mit Prozess vorausgewählt
        [HttpGet("/Process/Start/{templateId}")]
        [HttpGet("/Process/Start")]
        public async Task<IActionResult> Start(int? templateId)
        {
            if (await UserCanStartProcesses())
            {
                await AddModelsToViewData();
                StartProcessViewModel startProcessViewModel = new StartProcessViewModel(new Process());

                ApplicationUser defaultSupervisor = await GetCurrentUser();
                startProcessViewModel.Supervisor = defaultSupervisor;

                if (templateId != null && templateId > 0)
                {
                    bool processExists = _context.ProcessTemplates.ToList().Any(p => p.Id == templateId);

                    if (processExists)
                    {
                        ProcessTemplate template =
                            await _processTemplateContainer.GetProcessTemplateByIdAsync((int)templateId);

                        // template von context lösen um Veränderungen zu ignorieren
                        _context.Entry(template).State = EntityState.Detached;

                        template.AssignmentTemplates.ForEach(a => _context.Entry(a).State = EntityState.Detached);
                        template.AssignmentTemplates.ForEach(a =>
                        {
                            if (a.ForContractsList != null)
                            {
                                a.ForContractsList.ForEach(c => _context.Entry(c).State = EntityState.Detached);
                                a.ForContractsList.ForEach(c => c.AssignmentsTemplates = null);
                                a.ForContractsList.ForEach(c => c.Assignments = null);
                            }

                            if (a.ForDepartmentsList != null)
                            {
                                a.ForDepartmentsList.ForEach(d => _context.Entry(d).State = EntityState.Detached);
                                a.ForDepartmentsList.ForEach(d => d.AssignmentsTemplates = null);
                                a.ForDepartmentsList.ForEach(d => d.Assignments = null);
                            }
                        });

                        template.RolesWithAccess.ForEach(r => r.ProcessTemplates = null);

                        startProcessViewModel.Template = template;

                        IList<string> userRoles = await _userManager.GetRolesAsync(defaultSupervisor);
                        List<string> rolesWithAccess = new List<string> { };
                        template.RolesWithAccess.ForEach(r => rolesWithAccess.Add(r.Name));

                        bool hasAccess =
                            rolesWithAccess.Intersect(userRoles.ToList()).Any()
                            || User.IsInRole("Administrator");

                        if (hasAccess)
                        {
                            return View(startProcessViewModel);
                        }
                    }
                }
                else
                {
                    if (TempData["startProcessViewModel"] != null)
                    {
                        string jsonViewModel = TempData["startProcessViewModel"] as string;
                        startProcessViewModel = JsonConvert.DeserializeObject<StartProcessViewModel>(
                            jsonViewModel
                        );
                    }
                    
                    return View(startProcessViewModel);
                }
            }

            return RedirectToAction("Index");
        }
        
        [HttpPost("/Process/Start/{templateId}")]
        [HttpPost("/Process/Start")]
        public async Task<IActionResult> Start(
            [Bind(
                "Title, Description, DueDate, Template, AssignmentTemplates, Supervisor, WorkerOfReference, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                StartProcessViewModel startProcessViewModel
        )
        {
            if (await UserCanStartProcesses())
            {
                if (startProcessViewModel == null)
                {
                    throw new ArgumentNullException(nameof(startProcessViewModel));
                }

                if (startProcessViewModel.WorkerOfReference == null)
                {
                    ModelState.AddModelError("WorkerOfReference", "WorkerOfReference is required");
                }

                if (startProcessViewModel.Supervisor == null)
                {
                    ModelState.AddModelError("Supervisor", "Supervisor is required");
                }

                if (startProcessViewModel.Template == null)
                {
                    ModelState.AddModelError("Template", "Template is required");
                }

                if (startProcessViewModel.AssignmentTemplates == null)
                {
                    throw new ArgumentNullException("AssignmentTemplates");
                }

                await AddModelsToViewData();

                ModelState.Remove("Template.ContractOfRefWorker.Label");
                ModelState.Remove("Template.DepartmentOfRefWorker.Name");

                if (!ModelState.IsValid)
                {
                    // ModelState ist nicht valid
                    // Alle Fehler auflisten

                    foreach (var modelStateEntry in ModelState.Values)
                    {
                        foreach (var error in modelStateEntry.Errors)
                        {
                            // Validationfehlern loggen
                            var errorMessage = error.ErrorMessage;
                            var exception = error.Exception;
                        }
                    }

                    //return BadRequest(ModelState);
                    return View("Start", startProcessViewModel);
                }

                try
                {

                    startProcessViewModel.AssignmentTemplates = GetFilteredAssignmentTemplates(
                        startProcessViewModel.AssignmentTemplates,
                        startProcessViewModel.ContractOfRefWorker.Id,
                        startProcessViewModel.DepartmentOfRefWorker.Id
                    );
                    
                    ActiveProcess newProcess = startProcessViewModel.ToProcess();

                    startProcessViewModel
                        .AssignmentTemplates.Where(t => t.ProcessTemplateId == null)
                        .ToList()
                        .ForEach(t => _assignmentTemplateContainer.DeleteAssignmentTemplate(t.Id));

                    newProcess.WorkerOfReference = await _userManager.FindByIdAsync(
                        startProcessViewModel.WorkerOfReference.Id
                    );
                    newProcess.Supervisor = await _userManager.FindByIdAsync(
                        startProcessViewModel.Supervisor.Id
                    );
                    
                    newProcess.ContractOfRefWorker = _context.Contracts.Find(
                        startProcessViewModel.ContractOfRefWorker.Id
                    );
                    newProcess.DepartmentOfRefWorker = _context.Departments.Find(
                        startProcessViewModel.DepartmentOfRefWorker.Id
                    );

                    await _processContainer.AddProcessAsync(newProcess);
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException e)
                {
                    ModelState.AddModelError(e.InnerException.Message, e.Message);
                    Console.WriteLine(e.Message);
                    Console.WriteLine($"DbUpdateException: {e.InnerException.Message}");
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return View("Start", startProcessViewModel);
        }

        public async Task<IActionResult> StartRedirectToNewAssignment(
            [Bind(
                "Title, Template, Description, DueDate, Template, AssignmentTemplates, Supervisor, WorkerOfReference, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                StartProcessViewModel startProcessViewModel
        )
        {
            if (await UserCanStartProcesses())
            {
                string jsonViewModel = JsonConvert.SerializeObject(startProcessViewModel);
                TempData["startProcessViewModel"] = jsonViewModel;

                // Auf Create mit processId = 0 redirecten, da wir noch kein process haben
                return Redirect("/AssignmentTemplate/Create/0");
            }
            
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> StartRedirectEditAssignment(
            int assignmentTemplateId,
            [Bind(
                "Title, Template, Description, DueDate, Template, AssignmentTemplates, Supervisor, WorkerOfReference, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                StartProcessViewModel startProcessViewModel
        )
        {
            if (await UserCanStartProcesses())
            {
                string jsonViewModel = JsonConvert.SerializeObject(startProcessViewModel);
                TempData["startProcessViewModel"] = jsonViewModel;

                // Auf Create mit processId = 0 redirecten, da wir noch kein process haben
                return Redirect($"/AssignmentTemplate/Edit/{assignmentTemplateId}");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditRedirectToNewAssignment(
            [Bind(
                "Id, Title, Description, DueDate, WorkerOfReference, Supervisor, Assignments, AssignmentTemplates, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                EditProcessViewModel editProcessViewModel
        )
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync((int)editProcessViewModel.Id);

            if (process.Supervisor.Id == GetCurrentUserId() || User.IsInRole("Administrator"))
            {
                string jsonViewModel = JsonConvert.SerializeObject(editProcessViewModel);
                TempData["editProcessViewModel"] = jsonViewModel;
                Console.WriteLine(jsonViewModel);
                // Auf Create mit processId = 0, weil processId sich auf ProcessTemplateBezieht
                return Redirect("/AssignmentTemplate/Create/0");
            }
            
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditRedirectEditAssignment(
            int assignmentId,
            [Bind(
                "Id, Title, Description, DueDate, WorkerOfReference, Supervisor, Assignments, AssignmentTemplates, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                EditProcessViewModel editProcessViewModel
        )
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync((int)editProcessViewModel.Id);

            if (process.Supervisor.Id == GetCurrentUserId() || User.IsInRole("Administrator"))
            {
                string jsonViewModel = JsonConvert.SerializeObject(editProcessViewModel);
                TempData["editProcessViewModel"] = jsonViewModel;
                return Redirect($"/Assignment/Edit/{assignmentId}");
            }
            
            return RedirectToAction("Index");
        }

        [HttpGet("/Process/DetailRedirectEditAssignment/{processId}")]
        public async Task<IActionResult> DetailRedirectEditAssignment(
            int processId,
            int assignmentId
        )
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync(processId);

            if (process.Supervisor.Id == GetCurrentUserId() || User.IsInRole("Administrator"))
            {
                TempData["detailProcessId"] = processId;
                return Redirect($"/Assignment/Edit/{assignmentId}");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync(id);

            if (process.Supervisor.Id == GetCurrentUserId() || User.IsInRole("Administrator"))
            {
                EditProcessViewModel editProcessViewModel =
                    TempData["editProcessViewModel"] as EditProcessViewModel
                    ?? new EditProcessViewModel(process);

                editProcessViewModel.Id = id;

                bool hasAccess =
                    process.Supervisor.Id
                    == User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    || User.IsInRole("Administrator");

                if (process.IsArchived || !hasAccess)
                {
                    return RedirectToAction("Index");
                }

                await AddModelsToViewData();

                return View("Edit", editProcessViewModel);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("/Process/Edit/{id}")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "Id, Title, Description, DueDate, WorkerOfReference, Supervisor, Assignments, AssignmentTemplates, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                EditProcessViewModel editProcessViewModel
        )
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync(id);
            
            if (process.Supervisor.Id == GetCurrentUserId() || User.IsInRole("Administrator"))
            {
                List<Assignment> newAssignments = new List<Assignment> { };
                //beginn codeownership Jan Pfluger
                List<AssignmentTemplate> assignmentTemplates = new List<AssignmentTemplate>();
                assignmentTemplates = editProcessViewModel
                    .AssignmentTemplates.Where(temp =>
                        (
                            temp.ForContractsList == null
                            || temp.ForContractsList.Contains(editProcessViewModel.ContractOfRefWorker)
                            && (
                                temp.ForDepartmentsList == null
                                || temp.ForDepartmentsList.Contains(
                                    editProcessViewModel.DepartmentOfRefWorker
                                )
                            )
                        )
                    )
                    .ToList();

                foreach (AssignmentTemplate temp in assignmentTemplates)
                {
                    switch (temp.AssigneeType)
                    {
                        case AssigneeType.SUPERVISOR:
                            newAssignments.Add(
                                temp.ToAssignment(
                                    editProcessViewModel.Supervisor,
                                    editProcessViewModel.DueDate
                                )
                            );
                            break;
                        case AssigneeType.WORKER_OF_REF:
                            newAssignments.Add(
                                temp.ToAssignment(
                                    editProcessViewModel.WorkerOfReference,
                                    editProcessViewModel.DueDate
                                )
                            );
                            break;
                        default:
                            newAssignments.Add(temp.ToAssignment(null, editProcessViewModel.DueDate));
                            break;
                    }
                }

                //end codeownership Jan Pfluger

                if (editProcessViewModel.Assignments == null)
                {
                    editProcessViewModel.Assignments = new List<Assignment> { };
                }

                List<Assignment> currentAssignments = new List<Assignment> { };
                foreach (Assignment assignment in editProcessViewModel.Assignments)
                {
                    Assignment a = _context.Assignments.Find(assignment.Id);
                    currentAssignments.Add(a);
                }

                foreach (Assignment assignment in newAssignments)
                {
                    if (currentAssignments.Find(a => a.Title == assignment.Title) == null)
                    {
                        currentAssignments.Add(assignment);
                    }
                }

                newAssignments.AddRange(editProcessViewModel.Assignments);

                await AddModelsToViewData();

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("Modelstate Invalid");
                    return View("Edit", editProcessViewModel);
                }

                try
                {
                    ApplicationUser sup = await _userManager.FindByIdAsync(
                        editProcessViewModel.Supervisor.Id
                    );
                    ApplicationUser worker = await _userManager.FindByIdAsync(
                        editProcessViewModel.WorkerOfReference.Id
                    );

                    Contract contract = _context.Contracts.Find(
                        editProcessViewModel.ContractOfRefWorker.Id
                    );
                    Department department = _context.Departments.Find(
                        editProcessViewModel.DepartmentOfRefWorker.Id
                    );

                    await _processContainer.UpdateProcessAsync(
                        id,
                        editProcessViewModel.Title,
                        editProcessViewModel.Description,
                        currentAssignments,
                        sup,
                        worker,
                        contract,
                        department,
                        editProcessViewModel.DueDate
                    );

                    return RedirectToAction("Index");
                }
                catch (DbUpdateException e)
                {
                    ModelState.AddModelError("", e.InnerException?.Message ?? e.Message);
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.InnerException?.Message);
                }

                return View("Edit", editProcessViewModel);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detail(int id)
        {
            ApplicationUser user = await GetCurrentUser();
            List<string> roles = (List<string>) await _userManager.GetRolesAsync(user);
            ActiveProcess process = await _processContainer.GetProcessByIdAsync(id);
            bool canViewProcessDetails = false;

            foreach (var a in process.Assignments)
            {
                if (await UserIsAssignee(a))
                {
                    canViewProcessDetails = true;
                }
            }
            
            if (process.Supervisor.Id == user.Id || process.WorkerOfReference.Id == user.Id ||
                User.IsInRole("Administrator") || canViewProcessDetails)
            {

                DetailProcessViewModel detailProcessViewModel = new DetailProcessViewModel(process);

                return View("Detail", detailProcessViewModel);
            }

            return RedirectToAction("Index");
        }
        
        /*
        [HttpGet("/Process/UpdateAssignmentStatus/{processId}")]
        public async Task<IActionResult> UpdateAssignmentStatus(
            int processId,
            [FromQuery] int assignmentId,
            [FromQuery] AssignmentStatus status
        )
        {

            ActiveProcess process = await _processContainer.GetProcessByIdAsync(processId);
            ApplicationUser user = await GetCurrentUser();
            List<string> roles = (List<string>) await _userManager.GetRolesAsync(user);

            foreach (Assignment a in process.Assignments)
            {
                if (a.Id == assignmentId && ( await UserIsAssignee(a) || User.IsInRole("Administrator")))
                {
                    a.Status = status;
                }
            }

            await _processContainer.UpdateProcessAsync(
                processId,
                process.Title,
                process.Description,
                process.Assignments,
                process.Supervisor,
                process.WorkerOfReference,
                process.ContractOfRefWorker,
                process.DepartmentOfRefWorker,
                process.DueDate
            );

            return RedirectToAction("Detail", new { id = processId });
        }
        */

        [HttpPost("/Process/Stop/{id}")]
        public async Task<IActionResult> Stop(int id)
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync(id);
            if (process.Supervisor.Id == GetCurrentUserId() || User.IsInRole("Administrator"))
            {
                try
                {
                    await _processContainer.StopProcess(id);
                }
                catch (DbUpdateException e)
                {
                    Console.WriteLine(e.InnerException.Message);
                    ModelState.AddModelError("", e.InnerException.Message);
                }
            }

            return RedirectToAction("Index");
        }

        public List<AssignmentTemplate> GetFilteredAssignmentTemplates(List<AssignmentTemplate> assignmentTemplates, int contractId, int departmentId)
        {
            Console.WriteLine($"ASSIGNMENT COUNT: {assignmentTemplates.Count()}");
            
            List<AssignmentTemplate> filteredAssignmentTemplates = new List<AssignmentTemplate>();
            assignmentTemplates.ForEach(a =>
            {
                var contractMatch = false;
                var departmentMatch = false;
                if ((a.ForContractsList != null && a.ForContractsList.Any() && a.ForContractsList.Select(c => c.Id).Contains(contractId)) ||
                    (a.ForContractsList == null || !a.ForContractsList.Any())
                    )
                {
                    contractMatch = true;
                }

                if (a.ForDepartmentsList != null && a.ForDepartmentsList.Any() && a.ForDepartmentsList.Select(d => d.Id).Contains(departmentId) ||
                    (a.ForDepartmentsList == null || !a.ForDepartmentsList.Any())
                    )
                {
                    departmentMatch = true;
                }

                if (contractMatch && departmentMatch)
                {
                    filteredAssignmentTemplates.Add(a);
                }
                else
                {
                    Console.WriteLine($"IGNORING ASSIGNMENTTEMPLE: {a.Title}\nCONTRACT: {a.ForContractsList.FirstOrDefault().Label}\nDEPARTMENT: {a.ForDepartmentsList.FirstOrDefault().Name}");
                }
            });
            return filteredAssignmentTemplates;
        }

        public async Task AddModelsToViewData()
        {
            ApplicationUser user = await GetCurrentUser();
            List<ApplicationUser> users = _userManager.Users.Where(u => u.LockoutEnd == null).ToList();
            List<ProcessTemplate> processTemplates =
                await _processTemplateContainer.GetProcessListByAccessRights(user.UserName);
            
            processTemplates.ForEach(p => _context.Entry(p).State = EntityState.Detached);

            processTemplates.ForEach(p => p.AssignmentTemplates.ForEach(a =>
                {
                    _context.Entry(a).State = EntityState.Detached;
                    
                    // Liste leeren um Json serialization Fehler zu vermeiden
                    p.RolesWithAccess.ForEach(r => r.ProcessTemplates = null);
                    
                    if (a.ForContractsList != null)
                    {
                        a.ForContractsList.ForEach(c => c.Assignments = null);
                        a.ForContractsList.ForEach(c => c.AssignmentsTemplates = null);
                    }
                    
                    if (a.ForDepartmentsList != null)
                    {
                        a.ForDepartmentsList.ForEach(c => c.Assignments = null);
                        a.ForDepartmentsList.ForEach(c => c.AssignmentsTemplates = null);
                    }
                })
            );
            
            List<Assignment> assignments = _context.Assignments.ToList();
            assignments.ForEach(a => a.ForContractsList.ForEach(c => c.Assignments = null));
            assignments.ForEach(a => a.ForContractsList.ForEach(c => c.AssignmentsTemplates = null));
            assignments.ForEach(a => a.ForDepartmentsList.ForEach(d => d.Assignments = null));
            assignments.ForEach(a => a.ForDepartmentsList.ForEach(d => d.AssignmentsTemplates = null));

            List<Contract> contracts = _context.Contracts.ToList();
            contracts.ForEach(c => c.AssignmentsTemplates = null);
            contracts.ForEach(c => c.Assignments = null);
            List<Department> departments = _context.Departments.ToList();
            departments.ForEach(d => d.AssignmentsTemplates = null);
            departments.ForEach(d => d.Assignments = null);

            ViewData["Users"] = users;
            ViewData["ProcessTemplates"] = processTemplates;
            ViewData["Assignments"] = assignments;
            ViewData["Contracts"] = contracts;
            ViewData["Departments"] = departments;
        }

        public async Task<bool> UserCanStartProcesses()
        {
            ApplicationUser user = await GetCurrentUser();
            var roles = await _userManager.GetRolesAsync(user);
            List<ProcessTemplate> processTemplates = await _processTemplateContainer.GetProcessTemplatesAsync();
                //await _processTemplateContainer.GetProcessListByAccessRights(user.UserName);
            if (User.IsInRole("Administrator"))
            {
                return true;
            }
            
            foreach (var template in processTemplates)
            {
                if (template.RolesWithAccess.Select(r => r.Name).Intersect(roles).Any())
                {
                    return true;
                }
            }
                
            return false;
        }

        public async Task<bool> UserIsAssignee(Assignment assignment)
        {
            ApplicationUser user = await GetCurrentUser();
            List<string> roles = (List<string>) await _userManager.GetRolesAsync(user);
            
            return ((assignment.Assignee != null && assignment.Assignee.Id == user.Id) 
                    || (assignment.AssignedRole != null && roles.Contains(assignment.AssignedRole.Name)));
        }

        public string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        public async Task<ApplicationUser?>  GetCurrentUser()
        {
            string? userId = GetCurrentUserId();
            if (userId != null)
            {
                ApplicationUser? user = await _userManager.FindByIdAsync(userId);
                return user;
            }
            return  null;
        }
    }
}
