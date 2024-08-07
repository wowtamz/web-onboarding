//-------------------------
// Author: Michael Adolf
//-------------------------

using System.ComponentModel.DataAnnotations;

namespace SoPro24Team06.ViewModels
{
    /// <summary>
    /// ViewModel for changing the password
    /// Email: Email address of the user
    /// Password: New password
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Adresse")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Passwort ist erforderlich")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            ErrorMessage = "Das {0} muss mindestens {2} Zeichen lang sein.",
            MinimumLength = 6
        )]
        [Display(Name = "Passwort")]
        public string Password { get; set; }
    }
}
