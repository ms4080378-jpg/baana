namespace elbanna.Models
{
    public class st_userPermission
    {
        public int id { get; set; }
        public int screen { get; set; }
        public int userId { get; set; }

        public bool canAdd { get; set; }
        public bool canEdit { get; set; }
        public bool canDelete { get; set; }
        public bool canPrint { get; set; }
    }
}
