namespace elbanna.Models.ViewModels
{
    public class UserPermissionsVM
    {
        public int UserId { get; set; }

        public List<PermissionRowVM> Screens { get; set; }
        public List<PermissionRowVM> CostCenters { get; set; }
    }

}
