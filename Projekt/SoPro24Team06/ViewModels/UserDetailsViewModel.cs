using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SoPro24Team06.Enums;

//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.ViewModels
{
    /// <summary>
    /// ViewModel for the user details
    /// FullName: Full name of the user
    /// Email: Email address of the user
    /// Status: Status of the user
    /// SelectedRoles: List of selected roles
    /// Roles: List of all roles the user will have
    /// </summary>
    public class UserDetailsViewModel
    {
        [Required]
        [Display(Name = "Voller Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Adresse")]
        public string Email { get; set; }

        public string OriginalEmail { get; set; }

        [Required]
        public UserStatus Status { get; set; }

        [Required]
        [Display(Name = "Ausgewählte Rollen")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        public List<string> Roles { get; set; } = new List<string>();
    }
}
