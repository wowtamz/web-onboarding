//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.ViewModels
{
    /// <summary>
    /// Constructor for the UserRolesViewModel
    /// Includes a User and the roles of the user
    /// UserId: Id of the user
    /// Email: Email address of the user
    /// FullName: Full name of the user
    /// Roles: List of roles of the user
    /// </summary>
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
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}
