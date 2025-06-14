﻿//-------------------------
// Author: Tamas Varadi
//-------------------------
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoPro24Team06.ViewModels;

namespace SoPro24Team06.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Redirect to assignments overview view
        /// </summary>
        /// <returns>Index view of AssignmentController</returns>
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Assignment");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}
