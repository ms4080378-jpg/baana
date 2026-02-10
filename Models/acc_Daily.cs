using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_Daily")]
    public class acc_Daily
    {
        [Key]
        public int id { get; set; }

        public DateTime? processDate { get; set; }

        public int? costcenterId { get; set; }
        public string? costcenter { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public int? invoiceId { get; set; }

        public string? dealerCode { get; set; }
        public string? dealer { get; set; }

        public decimal? total { get; set; }
        public decimal? discount { get; set; }
        public decimal? net { get; set; }

        public string? fromAcc { get; set; }
        public string? toAcc { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? invoiceCode { get; set; }
        public string? notes { get; set; }
        [NotMapped]
        public string? lastUpdateUserName { get; set; }


        public bool? isReviewed { get; set; }
        public bool? isAllowed { get; set; }

        public int? insertUserId { get; set; }
        public DateTime? insertDate { get; set; }

        public int? lastUpdateUserId { get; set; }
        public DateTime? lastUpdateDate { get; set; }
    }
}
