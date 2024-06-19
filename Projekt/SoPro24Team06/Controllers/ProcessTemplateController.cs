using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    public class ProcessTemplateController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ILogger<ProcessTemplateController> _logger;

        public ProcessTemplateController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<ProcessTemplateController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        public IActionResult Index()
        {
            ProcessTemplateListViewModel model = new ProcessTemplateListViewModel();
            return View("~/Views/ProcessTemplates/Index.cshtml", model);
        }

        public IActionResult Create()
        {
            ProcessTemplateViewModel model = new ProcessTemplateViewModel();
            return View("~/Views/ProcessTemplates/Edit.cshtml", model);
        }

        public IActionResult Edit()
        {
            ProcessTemplateViewModel model = new ProcessTemplateViewModel();
            return View("~/Views/ProcessTemplates/Edit.cshtml", model);
        }

        public IActionResult Delete()
        {
            return View("~/Views/ProcessTemplates/Index.cshtml");
        }
    }
}
