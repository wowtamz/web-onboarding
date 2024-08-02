using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SoPro24Team06.Enums;

//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.ViewModels
{
    /// <summary>
    /// ViewModel for creating a user
    /// FullName: Full name of the user
    /// Email: Email address of the user
    /// Password: Password of the user
    /// Status: Status of the user (Locked, Active)
    /// SelectedRoles: List of selected roles
    /// Roles: List of all roles the user will have
    /// </summary>
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Voller Name ist erforderlich")]
        [Display(Name = "Voller Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email Adresse ist erforderlich")]
        [EmailAddress(ErrorMessage = "Ungültige Email Adresse")]
        [Display(Name = "Email Adresse")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Passwort ist erforderlich")]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserStatus Status { get; set; }

        [Required(ErrorMessage = "Mindestens eine Rolle muss ausgewählt werden")]
        [Display(Name = "Ausgewählte Rollen")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        public List<string> Roles { get; set; } = new List<string>();
    }
}
