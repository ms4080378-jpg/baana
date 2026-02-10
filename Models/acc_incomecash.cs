using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_incomecash")]
    public class acc_incomecash
    {
        [Key]
        public int id { get; set; }

        // =========================
        // Main Data
        // =========================
        public string payer { get; set; }

        public string costcenter { get; set; }

        public int? costcenterId { get; set; }

        public decimal? balance { get; set; }

        public string notes { get; set; }

        public DateTime? date { get; set; }

        // =========================
        // Audit
        // =========================
        public DateTime? insertDate { get; set; }

        public int? insertUserId { get; set; }

        public DateTime? lastUpdateDate { get; set; }

        public int? lastUpdateUserId { get; set; }

        // =========================
        // Status
        // =========================
        public bool? isReviewed { get; set; }

        public bool? isCheque { get; set; }

        public int? chequeId { get; set; }
    }
}
