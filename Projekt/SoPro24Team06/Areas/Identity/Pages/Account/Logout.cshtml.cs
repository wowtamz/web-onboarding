using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoPro24Team06.Models;
using System.Threading.Tasks;

//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Model for the Logout page
    /// </summary>
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        /// <summary>
        /// Logs out the User and redirects to the Index page
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }
    }
}
