using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_custody")]
    public class acc_custody
    {
        public int id { get; set; }

        public string custody { get; set; }

        public DateTime? date { get; set; }

        public decimal balance { get; set; }

        public string notes { get; set; }

        public bool? isReviewed { get; set; }

        public int? custodyId { get; set; }   // FK (nullable ✔)

        public int? insertUserId { get; set; }

        public DateTime? insertDate { get; set; }

        public int? lastUpdateUserId { get; set; }

        public DateTime? lastUpdateDate { get; set; }
    }
}
