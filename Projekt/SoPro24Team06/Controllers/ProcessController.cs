using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using SoPro24Team06.Containers;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;

using ActiveProcess = SoPro24Team06.Models.Process;


namespace SoPro24Team06.Controllers
{
    public class ProcessController : Controller
    {
        public ProcessContainer _processContainer = new ProcessContainer(new ModelContext());

        public async Task<IActionResult> Index()
        {

            //TODO: get Processes from Container with : await _processContainer.GetProcessesAsync();
            AssignmentTemplate aTemp = new AssignmentTemplate("", "", new DueTime("ASAP", 0,0,0), new List<Department> {}, new List<Contract> {}, AssigneeType.SUPERVISOR, new List<IdentityRole> {});
            Assignment a = new Assignment(aTemp, DateTime.Now);
            
            List<ActiveProcess> processList = new List<ActiveProcess> { 
                new ActiveProcess("Onboarding", "Keine Beschreibung", new List<Assignment> {a}, new ApplicationUser {FullName = "Zeus von Olymp"}, new ApplicationUser {FullName = "Depp vom Dienst"}, new Contract("Vollzeit"), new Department("IT")),
                new ActiveProcess("Onboarding", "Bob hat viel zu tun", new List<Assignment> {a}, new ApplicationUser {FullName = "Bob der Baumeister"}, new ApplicationUser {FullName = "Hans Peter"}, new Contract("Praktikant"), new Department("Marketing")),
            };

            ProcessListViewModel processListViewModel = new ProcessListViewModel(processList);
            return View(processListViewModel);
        }
        public async Task<IActionResult> Start()
        {
            return View("~/Views/Process/Start.cshtml");
        }

        public async Task<IActionResult> Edit()
        {
            return View("~/Views/Process/Edit.cshtml");
        }
    }
}