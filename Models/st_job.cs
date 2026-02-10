using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("st_job")] // 👈 الاسم الحقيقي في DB

    public class st_job
    {
        public int id { get; set; }
        public string job { get; set; }
    }
}
