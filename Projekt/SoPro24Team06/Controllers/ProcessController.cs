//-------------------------
// Author: Tamas Varadi
//-------------------------

using System.Collections.Immutable;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
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
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            List<ActiveProcess> userProcessList = await _processContainer.GetProcessesOfUserAsync(
                userId
            );
            
            List<ActiveProcess> myProcessList = new List<ActiveProcess> {};
            List<ActiveProcess> archivedProcessList = new List<ActiveProcess> {};
            
            foreach (var process in userProcessList)
            {
                if (process.IsArchived)
                {
                    archivedProcessList.Add(process);
                }
                else
                {
                    myProcessList.Add(process);
                }
            }

            ViewData["MyProcesses"] = myProcessList;

            ViewData["ArchivedProcesses"] = archivedProcessList;

            if (User.IsInRole("Administrator"))
            {
                List<ActiveProcess> ProcessList = await _processContainer.GetProcessesAsync();
                ViewData["AllProcesses"] = ProcessList.Where(p => !p.IsArchived);
                ViewData["AllArchivedProcesses"] = ProcessList.Where(p => p.IsArchived);
            }

            ProcessListViewModel processListViewModel = new ProcessListViewModel(myProcessList);
            return View(processListViewModel);
        }

        // Vorgang starten Seite mit ohne Prozess vorausgewählt
        [HttpGet("/Process/Start")]
        public async Task<IActionResult> Start()
        {
            await AddModelsToViewData();

            StartProcessViewModel startProcessViewModel = new StartProcessViewModel(new Process());

            return View(startProcessViewModel);
        }

        // Vorgang starten Seite mit Prozess vorausgewählt
        [HttpGet("/Process/Start/{templateId}")]
        public async Task<IActionResult> Start(int templateId)
        {
            Console.WriteLine($"Template ID: {templateId}");
            await AddModelsToViewData();

            StartProcessViewModel startProcessViewModel = new StartProcessViewModel(new Process());

            if (templateId > 0)
            {
                ProcessTemplate template =
                    await _processTemplateContainer.GetProcessTemplateByIdAsync(templateId);
                startProcessViewModel.Template = template;
            }

            return View(startProcessViewModel);
        }
        
        [HttpPost("/Process/Start/{templateId}")]
        public async Task<IActionResult> StartWithTemplate(
            [Bind(
                "Title, Description, Template, AssignmentTemplates, Supervisor, WorkerOfReference, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                StartProcessViewModel startProcessViewModel
        )
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
                ActiveProcess newProcess = startProcessViewModel.ToProcess();

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

            return View("Start", startProcessViewModel);
        }
        
        

        [HttpPost("/Process/Start")]
        public async Task<IActionResult> Start(
            [Bind(
                "Title, Description, Template, AssignmentTemplates, Supervisor, WorkerOfReference, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                StartProcessViewModel startProcessViewModel
        )
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
                ActiveProcess newProcess = startProcessViewModel.ToProcess();

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

            return View("Start", startProcessViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddAssignmentTemplate(
            [FromBody] AssignmentTemplate template,
            int index
        )
        {
            return Json(
                new
                {
                    success = true,
                    title = template.Title,
                    assignee = template.AssigneeType,
                    duein = template.DueIn.Label,
                    html = GenerateHtmlForAssignmentTemplate(template, index)
                }
            );
        }

        private string GenerateHtmlForAssignmentTemplate(AssignmentTemplate template, int index)
        {
            string inputs =
                $"<input type='hidden' name='AssignmentTemplates[{index}].Id' value='{template.Id}' />";
            inputs +=
                $"<input type='hidden' name='AssignmentTemplates[{index}].Title' value='{template.Title}' />";
            inputs +=
                $"<input type='hidden' name='AssignmentTemplates[{index}].Instructions' value='{template.Instructions}' />";

            inputs +=
                $"<input type='hidden' name='AssignmentTemplates[{index}].DueIn.Id' value='{template.DueIn.Id}' />";
            inputs +=
                $"<input type='hidden' name='AssignmentTemplates[{index}].DueIn.Label' value='{template.DueIn.Label}' />";
            inputs +=
                $"<input type='hidden' name='AssignmentTemplates[{index}].DueIn.Days' value='{template.DueIn.Days}' />";
            inputs +=
                $"<input type='hidden' name='AssignmentTemplates[{index}].DueIn.Weeks' value='{template.DueIn.Weeks}' />";
            inputs +=
                $"<input type='hidden' name='AssignmentTemplates[{index}].DueIn.Months' value='{template.DueIn.Months}' />";
            inputs +=
                $"<input type='hidden' name='AssignmentTemplates[{index}].AssigneeType' value='{template.AssigneeType}' />";

            if (template.AssignedRole != null)
            {
                inputs +=
                    $"<input type='hidden' name='AssignmentTemplates[{index}].AssignedRole.Id' value='{template.AssignedRole.Id}' />";
                inputs +=
                    $"<input type='hidden' name='AssignmentTemplates[{index}].AssignedRole.Name' value='{template.AssignedRole.Name}' />";
            }

            if (template.ForDepartmentsList != null)
            {
                for (int i = 0; i < template.ForDepartmentsList.Count; i++)
                {
                    var depId = template.ForDepartmentsList[i].Id;
                    var depName = template.ForDepartmentsList[i].Name;
                    inputs +=
                        $"<input type='hidden' name='AssignmentTemplates[{index}].ForDepartmentsList[{i}].Id' value='{depId}' />";
                    inputs +=
                        $"<input type='hidden' name='AssignmentTemplates[{index}].ForDepartmentsList[{i}].Name' value='{depName}' />";
                }
            }

            if (template.ForContractsList != null)
            {
                for (int i = 0; i < template.ForContractsList.Count; i++)
                {
                    var conId = template.ForContractsList[i].Id;
                    var conLabel = template.ForContractsList[i].Label;
                    inputs +=
                        $"<input type='hidden' name='AssignmentTemplates[{index}].ForContractsList[{i}].Id' value='{conId}' />";
                    inputs +=
                        $"<input type='hidden' name='AssignmentTemplates[{index}].ForContractsList[{i}].Label' value='{conLabel}' />";
                }
            }

            string _assignee = EnumHelper.GetDisplayName(template.AssigneeType);

            return $"<tr id='templateItem' name='templateItem{template.Id}'><td>{template.Title}</td> <td><a href='#'>Mehr anzeigen</a></td> <td>{_assignee}</td><td>Vollzeit</td><td>Personal</td><td>{template.DueIn.Label}</td> <td><button class='btn btn-danger' type='button' style='margin-left: 16px;' onclick='removeAssignmentTemplate(this, {template.Id})'>Entfernen</button></td> {inputs} </tr>";
        }

        public async Task<IActionResult> Edit(int id)
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync(id);
            EditProcessViewModel editProcessViewModel = new EditProcessViewModel(process);

            Console.WriteLine("GET EditProcessViewModel:");

            await AddModelsToViewData();

            return View("Edit", editProcessViewModel);
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
                        newAssignments.Add(temp.ToAssignment(editProcessViewModel.Supervisor));
                        break;
                    case AssigneeType.WORKER_OF_REF:
                        newAssignments.Add(
                            temp.ToAssignment(editProcessViewModel.WorkerOfReference)
                        );
                        break;
                    default:
                        newAssignments.Add(temp.ToAssignment(null));
                        break;
                }
            }
            //end codeownership Jan Pfluger

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
                    department
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

        public async Task<IActionResult> Detail(int id)
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync(id);
            DetailProcessViewModel detailProcessViewModel = new DetailProcessViewModel(process);

            return View("Detail", detailProcessViewModel);
        }

        [HttpPost("/Process/Stop/{id}")]
        public async Task<IActionResult> Stop(int id)
        {
            try
            {
                //await _processContainer.DeleteProcessAsync(id);
                await _processContainer.StopProcess(id);
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine(e.InnerException.Message);
                ModelState.AddModelError("", e.InnerException.Message);
            }

            return RedirectToAction("Index");
        }

        public async Task AddModelsToViewData()
        {
            List<ApplicationUser> users = _userManager.Users.ToList();
            List<ProcessTemplate> processTemplates =
                await _processTemplateContainer.GetProcessTemplatesAsync();

            processTemplates.ForEach(p => p.RolesWithAccess = new List<ApplicationRole> { });

            List<AssignmentTemplate> assignmentTemplates =
                _assignmentTemplateContainer.GetAllAssignmentTemplates();

             assignmentTemplates.ForEach(a => a.ProcessTemplateId = processTemplates.FirstOrDefault()?.Id ?? 0);

            // Replace with Containers
            List<Assignment> assignments = _context.Assignments.ToList();
            List<Contract> contracts = _context.Contracts.ToList();
            List<Department> departments = _context.Departments.ToList();

            ViewData["Users"] = users;
            ViewData["ProcessTemplates"] = processTemplates;
            ViewData["Assignments"] = assignments;
            ViewData["AssignmentTemplates"] = assignmentTemplates;
            ViewData["Contracts"] = contracts;
            ViewData["Departments"] = departments;
        }
    }
}
