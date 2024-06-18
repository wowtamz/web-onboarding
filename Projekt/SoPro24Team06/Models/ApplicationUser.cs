using Microsoft.AspNetCore.Identity;

namespace SoPro24Team06.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
