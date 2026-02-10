using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("pr_intrns")]
    public class pr_intrn
    {
        [Key]
        public int id { get; set; }

        public DateTime? processDate { get; set; }

        public int? costcenterId { get; set; }
        public string? costcenter { get; set; }

        public int? itemId { get; set; }
        public string? item { get; set; }

        public decimal? qty { get; set; }
        public decimal? unitPrice { get; set; }
        public decimal? total { get; set; }

        public DateTime? insertDate { get; set; }
        public DateTime? lastUpdateDate { get; set; }
        public int? insertUserId { get; set; }
        public int? lastUpdateUserId { get; set; }
    }
}
