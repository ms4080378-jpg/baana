using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_Dealer")]
    public class Dealer
    {
        [Key]
        public int id { get; set; }

        [Column("dealer")]
        public string dealer { get; set; } = null!;

        [Column("code")]
        public string? code { get; set; }

        [Column("nationalId")]
        public string? nationalId { get; set; }

        // ✅ لازم يكون MAPPED
        [Column("isStopped")]
        public bool isStopped { get; set; }

        [Column("isBank")]
        public bool isBank { get; set; }

        // خاص بالعرض فقط
        [NotMapped]
        public string Name => dealer;
    }
}
