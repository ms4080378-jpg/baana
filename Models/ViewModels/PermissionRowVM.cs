namespace elbanna.Models.ViewModels
{
    public class PermissionRowVM
    {
        public int RefId { get; set; }     // screenId أو costcenterId
        public string Name { get; set; }

        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
    }

}
