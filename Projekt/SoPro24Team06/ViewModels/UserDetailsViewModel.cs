using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SoPro24Team06.Enums;

namespace SoPro24Team06.ViewModels
{
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
