using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_CostCenter")]
    public class acc_CostCenter
    {
        [Key]
        public int id { get; set; }

        [Column("costCenter")]
        public string? costCenter { get; set; }

        public string? building { get; set; }
        public string? floor { get; set; }
        public string? floorUnit { get; set; }
    }
}
