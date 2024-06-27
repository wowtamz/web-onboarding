using System.ComponentModel.DataAnnotations;

namespace SoPro24Team06.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Adresse")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string Password { get; set; }
    }
}
