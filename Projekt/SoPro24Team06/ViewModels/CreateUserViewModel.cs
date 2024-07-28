using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.Web.CodeGeneration.CommandLine;
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
        [Required]
        [Display(Name = "Voller Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Adresse")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = String.Empty;

        [Required]
        public UserStatus Status { get; set; }

        [Required]
        [Display(Name = "Ausgewählte Rollen")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        public List<string> Roles { get; set; } = new List<string>();
    }
}
