using System.Collections.Generic;

namespace SoPro24Team06.Models
{
    public class UserRolesViewModel
    {
        public UserRolesViewModel()
        {
            Roles = new List<string>();
        }

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; }
    }
}
