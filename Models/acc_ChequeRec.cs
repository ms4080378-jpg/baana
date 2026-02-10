using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_ChequeRec")] // 👈 اسم الجدول الحقيقي في SQL
    public class acc_ChequeRec
    {
        public int id { get; set; }
        public DateTime? processDate { get; set; }
        public int? dealerId { get; set; }
        public int? bankId { get; set; }
        public string? chequeNo { get; set; }
        public int? chequeNote { get; set; }

        public decimal? total { get; set; }
        public string? notes { get; set; }
        public int? costcenterId { get; set; }
        public string? costcenter { get; set; }
        public string? fromAcc { get; set; }
        public string? toAcc { get; set; }
        public string? invoiceCode { get; set; }
        public bool? isReviewed { get; set; }
        public bool? isPaid { get; set; }
        public DateTime insertDate { get; set; }
        public int? insertUserId { get; set; }
        public DateTime? lastUpdateDate { get; set; }
        public int? lastUpdateUserId { get; set; }
    }
}
