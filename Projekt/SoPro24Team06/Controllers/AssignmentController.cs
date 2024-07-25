//beginn codeownership Jan Pfluger
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Container;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using SoPro24Team06.Helpers;

namespace SoPro24Team06.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AssignmentTemplateController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ProcessContainer _processContainer;
        private readonly AssingmentContainer _assignmentContainer;

        public AssignmentController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AssignmentTemplateController> logger,
            ApplicationDbContext context
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
            _processContainer = new ProcessContainer(_context);
            _assignmentContainer = new AssingmentContainer(_context);
        }

        public async Task<IActionResult> Index()
        {
            AssignmentIndexViewModel model = await GetAssignmentIndexViewModelAsync();

            int? selectedProcessId = HttpContext.Session.GetInt32("selectedProcessId");
            if (selectedProcessId.HasValue)
            {
                ViewData["selectedProcessId"] = selectedProcessId.Value;
            }

            string? currentList = HttpContext.Session.GetString("currentList");
            if (currentList != null)
            {
                ViewData["currentList"] = currentList;
            }

            string? sortingMethod = HttpContext.Session.GetString("sortingMethod");
            {
                ViewData["sortingMethod"] = sortingMethod;
            }
            return View("~/Views/Assignments/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignment([FromForm] AssignmentEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // if invalid return to View and show error Message
                return View(model);
            }

            if (
                model.Assignment.AssigneeType == AssigneeType.ROLES
                && model.Assignment.AssignedRole == null
            )
            {
                ModelState.AddModelError(
                    "Assignment.AssignedRole",
                    "Bitte wählen Sie eine Rolle aus"
                );
            }
            else if (
                model.Assignment.AssigneeType == AssigneeType.USER
                && model.Assignment.Assignee == null
            )
            {
                ModelState.AddModelError(
                    "Assignment.Assingee",
                    "Bitte wählen Sie einen Nutzer aus."
                );
            }

            if (model.SelectedProcessId == 0)
            {
                ModelState.AddModelError(
                    "SelectedProcessId",
                    "Bitte wählen Sie einen Vorgang aus."
                );
            }

            if (model.Assignment.DueDate == null)
            {
                ModelState.AddModelError("DueDate", "Bitte wählen sie ein Datum aus");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Assignment? assignment = await _context.Assignments.FirstOrDefaultAsync(a =>
                a.Id == model.Assignment.Id
            );

            if (assignment == null)
            {
                return NotFound();
            }

            assignment.Title = model.Assignment.Title;
            assignment.Instructions = model.Assignment.Instructions;
            assignment.DueDate = model.Assignment.DueDate;
            assignment.AssigneeType = model.Assignment.AssigneeType;
            assignment.Assignee = model.Assignment.Assignee;
            assignment.AssignedRole = model.Assignment.AssignedRole;
            assignment.Status = model.Assignment.Status;
            assignment.DueDate = model.Assignment.DueDate;

            Process? originalProcess = _context.Processes.FirstOrDefault(p =>
                p.Assignments.Any(a => a.Id == assignment.Id)
            );
            Process? selectedProcess = _context.Processes.FirstOrDefault(p =>
                p.Assignments.Any(a => a.Id == model.Assignment.Id)
            );

            if (
                originalProcess != null
                && selectedProcess != null
                && originalProcess != selectedProcess
            )
            {
                originalProcess.Assignments.Remove(assignment);
                selectedProcess.Assignments.Add(assignment);
                _context.Processes.Update(originalProcess);
                _context.Processes.Update(selectedProcess);
                await _context.SaveChangesAsync();
            }

            _context.Assignments.Update(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); //hier muss noch der richtige Redirect rein, je nach dem wo man es aufgerufen hat;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDetails([FromForm] AssignmentDetailsViewModel model)
        {
			if (ModelState.IsValid)
			{
				if (model.Assignment.AssigneeType == AssigneeType.ROLES)
				{
					if(model.SelectedRoleId == null)
					{
						ModelState.AddModelError(
							"Assignment.AssignedRole",
							"Bitte wählen Sie eine Rolle aus 1"
						);
					}
					else if(await _roleManager.FindByIdAsync(model.SelectedRoleId) == null){
						ModelState.AddModelError(
							"Assignment.AssignedRole",
							"Bitte wählen Sie eine Rolle aus 2"
						);
					}
				}
				else if (model.Assignment.AssigneeType == AssigneeType.USER)
				{
					if(model.SelectedUserId == null)
					{
						ModelState.AddModelError(
							"Assignment.Assingee",
							"Bitte wählen Sie einen Nutzer aus. 1"
						);
					}
					else if(await _userManager.FindByIdAsync(model.SelectedUserId) == null){
						ModelState.AddModelError(
							"Assignment.Assingee",
							"Bitte wählen Sie einen Nutzer aus. 2"
						);
					}
				}
				else
				{
					ModelState.AddModelError(
						"Assignment.AssigneeType",
						"Bitte wählen Sie eine Zuständigkeitsart"
					);
				}
				
				if(model.Assignment.Status == null)
				{
					ModelState.AddModelError
					(
						"Assignment.Status",
						"Bitte wählen sie einen Bearbeitungszustand aus"
					);
				}
			}

			if (ModelState.IsValid == false)
			{
				foreach (var modelStateEntry in ModelState.Values)
				{
					foreach (var error in modelStateEntry.Errors)
					{
						var errorMessage = error.ErrorMessage;
						var exception = error.Exception;
					}
				}
				return BadRequest(ModelState);
				if (model.Assignment == null) return NotFound();
            	List<Process> processList = await _processContainer.GetProcessesAsync();
            	Process? process = processList.FirstOrDefault(p => p.Assignments.Contains(model.Assignment));
				List<ApplicationUser> userList = _userManager.Users.ToList();

				foreach (ApplicationUser u in userList)
				{
					if (await _userManager.IsLockedOutAsync(u)) userList.Remove(u);
				}
				List<ApplicationRole> roleList =  _roleManager.Roles.ToList();

				model.InitialiseSelectList(userList, roleList);
                return View("~/Views/Assignments/AssignmentDetails.cshtml", model);
			}

            Assignment? assignment = await _context.Assignments.FirstOrDefaultAsync(a =>
                a.Id == model.Assignment.Id
            );
			ApplicationUser ? selectedUser = await _userManager.FindByIdAsync(model.SelectedUserId);
			ApplicationRole ? selectedRole = await _roleManager.FindByIdAsync(model.SelectedRoleId);

            if (assignment == null)
            {
                return NotFound();
            }

			assignment.Status = model.Assignment.Status;
			assignment.AssigneeType = model.Assignment.AssigneeType;
			if(assignment.AssigneeType == AssigneeType.ROLES)
			{
				assignment.AssignedRole = selectedRole;
				_context.Assignments.Update(assignment);
				_context.Entry(assignment).Reference(a => a.Assignee).CurrentValue = null;
			}
			if(assignment.AssigneeType == AssigneeType.USER)
			{
				assignment.AssignedRole = null;
				_context.Assignments.Update(assignment);
				_context.Entry(assignment).Reference(a => a.AssignedRole).CurrentValue = null;
			}
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //hier muss noch was geändert werden
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int assignmentId)
        {
            Assignment? assignment = _assignmentContainer.GetAssignmentById(assignmentId);
            if (assignment == null)
            {
                return NotFound();
            }
			
            List<Process> processList = await _processContainer.GetProcessesAsync();
            Process? process = processList.FirstOrDefault(p =>
                p.Assignments != null && p.Assignments.Contains(assignment)
            );
            int processId;
            if (process != null)
            {
                processId = process.Id;
            }
            else
            {

                return NotFound();
            }
            AssignmentEditViewModel model = new AssignmentEditViewModel(
                assignment,
                processId,
                processList,
                _userManager.Users.ToList(),
                _roleManager.Roles.ToList()
            );
            return View("~/Views/Assignments/EditAssignment.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int assignmentId)
        {
            Assignment? assignment = _assignmentContainer.GetAssignmentById(assignmentId);
            if (assignment == null) return NotFound();
            List<Process> processList = await _processContainer.GetProcessesAsync();
            Process? process = processList.FirstOrDefault(p => p.Assignments.Contains(assignment));
			List<ApplicationUser> userList = _userManager.Users.ToList();
			foreach (ApplicationUser u in userList)
			{
				if (await _userManager.IsLockedOutAsync(u)) userList.Remove(u);
			}
			List<ApplicationRole> roleList =  _roleManager.Roles.ToList();
            AssignmentDetailsViewModel model = new AssignmentDetailsViewModel(
                assignment,
                userList,
                roleList,
                process
            );
            return View("~/Views/Assignments/AssignmentDetails.cshtml", model);
        }

        public async Task<IActionResult> Delete()
        {
            return View("~/Views/Assignments/Index.cshtml");
        }

        [HttpPost]
        public IActionResult FilterAssignments(int selectedProcessId)
        {
            HttpContext.Session.SetInt32("selectedProcessId", selectedProcessId);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeTabel(string currentList)
        {
            HttpContext.Session.SetString("currentList", currentList);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SortAssignments(string sortingMethod)
        {
            HttpContext.Session.SetString("sortingMethod", sortingMethod);
            return RedirectToAction("Index");
        }

        private async Task<AssignmentIndexViewModel> GetAssignmentIndexViewModelAsync()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            List<string> roles = new List<string>(await _userManager.GetRolesAsync(user));

            List<Process> processList = await _processContainer.GetProcessesAsync();
            // überprüfung einfügen ob Process noch nicht Archiviert ist.
            List<Assignment> assignmentList = new List<Assignment>();

            switch (HttpContext.Session.GetString("currentList"))
            {
                case "RoleAssignment":
                    processList = processList
                        .Where(p => !p.IsArchived &&
                            p.Assignments.Any(a =>
                                a.AssignedRole != null && roles.Contains(a.AssignedRole.ToString())
                            )
                        )
                        .ToList();
					foreach(Process p in processList)
					{
						foreach(Assignment a in p.Assignments)
						{
							if(a.AssignedRole != null && roles.Contains(a.AssignedRole.ToString())) assignmentList.Add(a);
						}
					}
					assignmentList = _context.Assignments.ToList().Where(a => a.AssignedRole != null && roles.Contains(a.AssignedRole.Name)).ToList();
                    break;
				
                case "AllAssignments":
                    if (roles.Contains("Administrator"))
                    {
                        foreach(Process p in processList)
						{
							if(!p.IsArchived)
							{
								foreach(Assignment a in p.Assignments)
								{
									assignmentList.Add(a);
								}
							}
						}
						assignmentList = _context.Assignments.ToList();
					}
					else if (processList.Any(p => p.Supervisor == user))
					{
						processList = processList.Where(p => !p.IsArchived && (p.Supervisor == user || p.Assignments.Any(
							a => (a.Assignee != null && a.Assignee.Id == user.Id) || (a.AssignedRole != null && roles.Contains(a.AssignedRole.ToString()))
							))).ToList();
						foreach(Process p in processList)
						{
							if(p.Supervisor == user)
							{
								foreach(Assignment a in p.Assignments)
								{
									assignmentList.Add(a);
								}
								assignmentList = _context.Assignments.ToList();
							} 
							else
							{
									foreach(Assignment a in p.Assignments)
								{
									if((a.Assignee != null && a.Assignee == user) || (a.AssignedRole != null && roles.Contains(a.AssignedRole.ToString())))assignmentList.Add(a);
								}	
							}
						}
						assignmentList = _context.Assignments.ToList().Where(a => (a.Assignee != null && a.Assignee == user) || (a.AssignedRole != null && roles.Contains(a.AssignedRole.ToString()))).ToList();

					}
                    else
                    {
                        processList = processList
                            .Where(p =>
                                p.Assignments.Any(a =>
                                    (a.Assignee != null && a.Assignee.Id == user.Id) || (a.AssignedRole != null && roles.Contains(a.AssignedRole.ToString()))
                            	)
							)
                            .ToList();
						foreach(Process p in processList)
						{
							foreach(Assignment a in p.Assignments)
							{
								if(a.Assignee != null && a.Assignee == user) assignmentList.Add(a);
								else if (a.AssignedRole != null && roles.Contains(a.AssignedRole.ToString())) assignmentList.Add(a);
							}
						}
						assignmentList = _context.Assignments.ToList().Where(a => a.Assignee != null && a.Assignee == user).ToList();
						HttpContext.Session.SetString("currentList", "MyAssignments");
                    }
                    break;
                default:
                    processList = processList
                        .Where(p =>
                            p.Assignments.Any(a => a.Assignee != null && a.Assignee.Id == user.Id)
                        )
                        .ToList();

                    foreach(Process p in processList)
						{
							foreach(Assignment a in p.Assignments)
							{
								if(a.Assignee != null && a.Assignee == user) assignmentList.Add(a);
							}
						}
					assignmentList = _context.Assignments.ToList().Where(a => a.Assignee != null && a.Assignee == user).ToList();
                    HttpContext.Session.SetString("currentList", "MyAssignments");
                    break;
            }
            AssignmentIndexViewModel model = new AssignmentIndexViewModel(
                assignmentList,
                processList
            );

            string? sortingMethod = HttpContext.Session.GetString("sortingMethod");
            if (sortingMethod != null)
            {
                model.SortAssingments(sortingMethod);
            }

            int? selectedProcess = HttpContext.Session.GetInt32("selectedProcessId");
            if (selectedProcess.HasValue)
            {
                model.FilterAssignments(selectedProcess.Value);
            }

            return model;
        }
    }
}

//end codeownership Jan Pfluger
