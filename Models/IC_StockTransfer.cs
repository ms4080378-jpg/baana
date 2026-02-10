using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("IC_StockTransfer")]
    public class IC_StockTransfer
    {
        [Key]
        public int id { get; set; }

        // ===== Item =====
        public int itemid { get; set; }

        [Column("item")]
        public string? item { get; set; }

        // ===== From Cost Center =====
        public int costcenterId { get; set; }

        public string? costcenter { get; set; }

        // ===== To Cost Center =====
        public int costcenterToId { get; set; }

        public string? costcenterTo { get; set; }

        // ===== Process =====
        public DateTime? processDate { get; set; }

        public decimal? qty { get; set; }

        // ===== Audit =====
        public int? insertUserId { get; set; }

        public DateTime? insertDate { get; set; }

        public int? lastUpdateUserId { get; set; }

        public DateTime? lastUpdateDate { get; set; }
    }
}
