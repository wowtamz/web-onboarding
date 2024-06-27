namespace SoPro24Team06.ViewModels
{
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
