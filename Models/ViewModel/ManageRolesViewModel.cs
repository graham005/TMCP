namespace TMCP.Models.ViewModel
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<RoleViewModel> Roles { get; set; }
        public string  SelectedRoleId { get; set; }
    }
}
