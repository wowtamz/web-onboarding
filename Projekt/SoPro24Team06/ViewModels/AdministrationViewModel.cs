//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.ViewModels
{
    /// <summary>
    /// ViewModel for the administration view
    /// CreateUserViewModel: ViewModel for creating a new user
    /// UserRoles: List of all users with their roles
    /// Roles: List of all roles
    /// RoleViewModel: ViewModel for a role
    /// </summary>
    public class AdministrationViewModel
    {
        public AdministrationViewModel()
        {
            CreateUserViewModel = new CreateUserViewModel();
            UserRoles = new List<UserRolesViewModel>();
            Roles = new List<string>();
        }

        public CreateUserViewModel CreateUserViewModel { get; set; }
        public IEnumerable<UserRolesViewModel> UserRoles { get; set; }
        public List<string> Roles { get; set; }
        public RoleViewModel RoleViewModel { get; set; }
    }
}
