using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("con_invoice")]
    public class ConInvoice
    {
        [Key]
        public int id { get; set; }

        public string? invoiceCode { get; set; }
        public DateTime? date { get; set; }

        public string? invoiceType { get; set; }

        public string? costcenter { get; set; }
        public int? costcenterId { get; set; }

        public decimal? balance { get; set; }

        public string? factor { get; set; }
        public decimal? factorValue { get; set; }
        public int? insertUserId { get; set; }

        public decimal? net { get; set; }

        public string? payFor { get; set; }
        public int? payForId { get; set; }

        public bool? isRefund { get; set; }
        public bool? isReviewed { get; set; }

        public DateTime? insertDate { get; set; }
        public DateTime? lastUpdateDate { get; set; }
        public int? lastUpdateUserId { get; set; }
    }
}
