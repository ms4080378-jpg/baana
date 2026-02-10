namespace elbanna.Models
{
    public class st_UserCCPermission
    {
        public int id { get; set; }
        public int costcenter { get; set; }
        public int userId { get; set; }

        public bool canAdd { get; set; }
        public bool canEdit { get; set; }
        public bool canDelete { get; set; }
        public bool canPrint { get; set; }
    }
}
