namespace SoPro24Team06.Models
{
    public class UserManagementViewModel
    {
        public UserManagementViewModel()
        {
            CreateUserViewModel = new CreateUserViewModel();
            UserRoles = new List<UserRolesViewModel>();
        }

        public CreateUserViewModel CreateUserViewModel { get; set; }
        public IEnumerable<UserRolesViewModel> UserRoles { get; set; }
    }
}
