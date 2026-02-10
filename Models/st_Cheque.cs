using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("st_Cheque")]
    public class st_Cheque
    {
        [Key]
        public int id { get; set; }

        public long chequeNo { get; set; }     // رقم أول ورقة
        public int qty { get; set; }            // عدد الورقات

        public int? dealerId { get; set; }
        public string? notes { get; set; }
        public string? respons { get; set; }

        public int? insertUserId { get; set; }
        public DateTime? insertDate { get; set; }

        public int? lastUpdateUserId { get; set; }
        public DateTime? lastUpdateDate { get; set; }
    }
}
