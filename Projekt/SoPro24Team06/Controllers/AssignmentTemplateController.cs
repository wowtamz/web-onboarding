using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    [Authorize]
    public class AssignmentTemplateController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ILogger<AssignmentTemplateController> _logger;

        public AssignmentTemplateController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AssignmentTemplateController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        public IActionResult Index()
        {
            AssigmentTemplateListViewModel model = new AssigmentTemplateListViewModel();
            return View("~/Views/Assignments/Index.cshtml", model);
        }

        public IActionResult Create()
        {
            return View("~/Views/Assignments/Create.cshtml");
        }

        public IActionResult Edit()
        {
            return View("~/Views/Assignments/Edit.cshtml");
        }

        public async Task<IActionResult> Delete()
        {
            return View("~/Views/Assignments/Index.cshtml");
        }
    }
}
