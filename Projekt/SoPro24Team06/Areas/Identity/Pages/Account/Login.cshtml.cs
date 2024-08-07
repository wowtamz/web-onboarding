using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SoPro24Team06.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Model for the Login page
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Constructor for the Login Model
        /// </summary>
        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
            Input = new InputModel();
            ReturnUrl = string.Empty;
            ErrorMessage = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Input Model for the Login Page
        /// </summary>
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            //Brauchen wir das??

            [Display(Name = "Eingeloggt bleiben?")]
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Loads the Login Page
        /// </summary>
        /// <param name="returnUrl"> Redirection Url after Login </param>
        /// <returns></returns>
        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");

            // cookies löschen
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        /// <summary>
        /// Checks the Login Data and logs in the User
        /// </summary>
        /// <param name="returnUrl"> Redirection Url after Login </param>
        /// <returns></returns>
    
        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Benutzer Eingeloggt.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Benutzerkonto gesperrt.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    _logger.LogWarning("Login fehlgeschlagen.");
                    ModelState.AddModelError(string.Empty, "Login fehlgeschlagen.");
                    return Page();
                }
            }

            return Page();
        }

    }
}
