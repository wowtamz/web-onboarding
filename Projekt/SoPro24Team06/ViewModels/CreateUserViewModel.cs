using System.ComponentModel.DataAnnotations;

namespace SoPro24Team06.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
