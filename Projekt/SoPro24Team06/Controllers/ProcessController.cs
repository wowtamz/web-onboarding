using System.Collections.Immutable;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using ActiveProcess = SoPro24Team06.Models.Process;

namespace SoPro24Team06.Controllers
{
    [Authorize]
    public class ProcessController : Controller
    {
        public ProcessContainer _processContainer = new ProcessContainer(new ModelContext());
        public ProcessTemplateContainer _processTemplateContainer = new ProcessTemplateContainer();
        public AssignmentTemplateContainer _assignmentTemplateContainer =
            new AssignmentTemplateContainer();

        public async Task<IActionResult> Index()
        {
            List<ActiveProcess> processList = await _processContainer.GetProcessesAsync();

            ProcessListViewModel processListViewModel = new ProcessListViewModel(processList);
            return View(processListViewModel);
        }

        public async Task<IActionResult> Start([FromServices] UserManager<ApplicationUser> userManager, [FromQuery] ProcessViewModel? processViewModel)
        {

            ModelContext modelContext = new ModelContext();
            // Replace with Containers
            
            List<ApplicationUser> users = userManager.Users.ToList();
            List<ProcessTemplate> processTemplates = modelContext.ProcessTemplates.ToList();
            List<AssignmentTemplate> assignmentTemplates =
                modelContext.AssignmentTemplates.ToList();
            List<Contract> contracts = modelContext.Contracts.ToList();
            List<Department> departments = modelContext.Departments.ToList();

            ComposeProcessViewModel composeProcessViewModel =
                processViewModel.ToComposeProcessViewModel();

            composeProcessViewModel.AvailableUsers = users;
            composeProcessViewModel.AvailableProcessTemplates = processTemplates;
            composeProcessViewModel.AvailableAssignmentTemplates = assignmentTemplates;
            composeProcessViewModel.AvailableContracts = contracts;
            composeProcessViewModel.AvailableDepartments = departments;

            return View(composeProcessViewModel);
        }

        [HttpPost("/Process/Start")]
        public async Task<IActionResult> Start(
            [Bind(
                "Title, Description, WorkerOfReference, Supervisor, Assignments, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                ComposeProcessViewModel composeProcessViewModel
        )
        {
            Console.WriteLine("POST-REQ");
            if (ModelState.IsValid)
            {
                try
                {
                    await _processContainer.AddProcessAsync(composeProcessViewModel.ToProcess());
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException e)
                {
                    ModelState.AddModelError("", e.InnerException.Message);
                }
                return View(composeProcessViewModel);
            }

            return View(composeProcessViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> StartProcess(
            int templateId,
            string title,
            string description,
            int contractId,
            int departmentId,
            int[] assignmentTemplateArray
        )
        {
            Console.WriteLine("starting Process");

            ModelContext modelContext = new ModelContext();
            ProcessTemplate processTemplate =
                await _processTemplateContainer.GetProcessTemplateByIdAsync(templateId);
            Contract contract = await modelContext.Contracts.FindAsync(contractId);
            Department department = await modelContext.Departments.FindAsync(departmentId);

            List<AssignmentTemplate> assignmentTemplateList = new List<AssignmentTemplate> { };

            foreach (int id in assignmentTemplateArray)
            {
                AssignmentTemplate assignmentTemplate =
                    _assignmentTemplateContainer.GetAssignmentTemplate(id);
                assignmentTemplateList.Add(assignmentTemplate);
            }
            
            Console.WriteLine(title);
            Console.WriteLine(description);
            Console.WriteLine(processTemplate.Title);
            Console.WriteLine(contract.Label);
            Console.WriteLine(department.Name);
            
            Process newProcess = new Process(processTemplate);
            newProcess.Title = title;
            newProcess.Description = description;
            newProcess.ContractOfRefWorker = new Contract(contract.Label);
            newProcess.DepartmentOfRefWorker = new Department(department.Name);
            newProcess.WorkerOfReference = new ApplicationUser { FullName = "Benutzer"};
            newProcess.Supervisor = new ApplicationUser { FullName = "Admin"};
            newProcess.Assignments = assignmentTemplateList.ConvertAll(template =>
                template.ToAssignment(template)
            );

            Console.WriteLine(newProcess.Title);
            Console.WriteLine(newProcess.ContractOfRefWorker.Label);
            Console.WriteLine(newProcess.DepartmentOfRefWorker.Name);
            Console.WriteLine(newProcess.Assignments[0].Title);

            await _processContainer.AddProcessAsync(newProcess);
            
            
            return Json(new { redirectToUrl = Url.Action("Index", "Process") });
        }

        // Evtl irrelevates Code-Teil
        [HttpPost]
        public async Task<IActionResult> SelectWorkerOfRef(int id, string name) {
            
            return Json(new { success = true, html = name });
        }

        [HttpPost]
        public async Task<IActionResult> SelectContract(int id)
        {
            ModelContext modelContext = new ModelContext();
            Contract contract = await modelContext.Contracts.FindAsync(id);
            return Json(new { success = true, label = contract.Label });
        }

        [HttpPost]
        public async Task<IActionResult> SelectDepartment(int id)
        {
            ModelContext modelContext = new ModelContext();
            Department department = await modelContext.Departments.FindAsync(id);

            return Json(new { success = true, label = department.Name });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyProcessTemplate(int id) {

            ModelContext modelContext = new ModelContext();
            ProcessTemplate processTemplate = await modelContext.ProcessTemplates.FindAsync(id);

            int[] assignmentIds = new int[] { };

            foreach (AssignmentTemplate template in processTemplate.AssignmentTemplates)
            {
                assignmentIds.Append(template.Id);
            }

            // assignmentIds = {1, 2} for testing, should normally be empty
            /*
            int[] assignmentIds = new int[] {1, 2};
            
            */

            //test Vertrag & Abteilung
            /*
            processTemplate.ContractOfRefWorker.Id = 1;
            processTemplate.DepartmentOfRefWorker.Id = 1;
            */

            return Json(
                new
                {
                    success = true,
                    title = processTemplate.Title,
                    contractId = processTemplate.ContractOfRefWorker.Id,
                    departmentId = processTemplate.DepartmentOfRefWorker.Id,
                    assignments = assignmentIds
                }
            );
        }

        public async Task<IActionResult> AddAssignmentTemplate(int id) {
            
            ModelContext modelContext = new ModelContext();
            AssignmentTemplate assignmentTemplate =
                await modelContext.AssignmentTemplates.FindAsync(id);
            //List<object> assignments = processTemplate.AssignmentTemplates.To

            string _assignee = "Rollen";

            switch (assignmentTemplate.AssigneeType) 
            {
                case AssigneeType.ROLES:
                    _assignee = "Rollen";
                    break;
                
                case AssigneeType.SUPERVISOR:
                    _assignee = "Vorgangsverantwortlicher";
                    break;
                
                case AssigneeType.WORKER_OF_RER:
                    _assignee = "Bezugsperson";
                    break;
                
                default:
                    break;
            }

            return Json(
                new
                {
                    success = true,
                    title = assignmentTemplate.Title,
                    assignee = assignmentTemplate.AssigneeType,
                    duein = assignmentTemplate.DueIn.Label,
                    html = GenerateHtmlForAssignmentTemplate(assignmentTemplate, _assignee)
                }
            );
        }

        private string GenerateHtmlForAssignmentTemplate(AssignmentTemplate template, string assignee)
        {
            return $"<tr id='templateItem' name='templateItem{template.Id}'><td>{template.Title}</td> <td><a href='#'>Mehr anzeigen</a></td> <td>{assignee}</td><td>Vollzeit</td><td>Personal</td><td>{template.DueIn.Label}</td> <td><button class='btn btn-danger' type='button' style='margin-left: 16px;' onclick='removeAssignmentTemplate(this, {template.Id})'>Entfernen</button></td></tr>";
        }

        public async Task<IActionResult> Edit(int id)
        {
            ActiveProcess process = await _processContainer.GetProcessByIdAsync(id);
            ProcessViewModel processViewModel = new ProcessViewModel(process);
            return View("Edit", processViewModel);
        }

        [HttpPost("/Process/Edit/{id}")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "Id, Title, Description, StartDate, DueDate, WorkerOfReference, Supervisor, Assignments, ContractOfRefWorker, DepartmentOfRefWorker"
            )]
            [FromForm]
                ActiveProcess process
        )
        {
            ProcessViewModel processViewModel = new ProcessViewModel(process);

            if (!ModelState.IsValid)
            {
                return View("Edit", processViewModel);
            }

            try
            {
                await _processContainer.UpdateProcessAsync(
                    id,
                    process.Title,
                    process.Description,
                    process.Assignments,
                    process.Supervisor,
                    process.WorkerOfReference,
                    process.ContractOfRefWorker,
                    process.DepartmentOfRefWorker
                );
                return RedirectToAction("Index");
            }
            catch (DbUpdateException e)
            {
                ModelState.AddModelError("", e.InnerException?.Message ?? e.Message);
            }

            return View("Edit", processViewModel);
        }

        [HttpPost("/Process/Stop/{id}")]
        public async Task<IActionResult> Stop(int id)
        {
            try
            {
                await _processContainer.DeleteProcessAsync(id);
            }
            catch (DbUpdateException e)
            {
                ModelState.AddModelError("", e.InnerException.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
