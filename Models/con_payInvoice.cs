using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("con_payInvoice")] // 👈 الاسم الحقيقي في SQL
    public class con_payInvoice
    {
        public int id { get; set; }
        public DateTime? date { get; set; }
        public string? invoiceCode { get; set; }

        public int costcenterId { get; set; }
        public string? costcenter { get; set; }

        public decimal? paid { get; set; }
        public string? notes { get; set; }

        public bool? isReviewed { get; set; }
        public bool? isPaid { get; set; }

        public int? insertUserId { get; set; }
        public DateTime? insertDate { get; set; }

        public int? lastUpdateUserId { get; set; }
        public DateTime? lastUpdateDate { get; set; }
    }
}
