using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_custody")]
    public class acc_custody_stage
    {
        public int id { get; set; }

        [Required]
        public string custodyName { get; set; }   // اسم العهدة
        public int costCenterId { get; set; }

        [Required]
        public DateTime custodyDate { get; set; }

        public decimal amount { get; set; }

        public string notes { get; set; }

        public bool isReviewed { get; set; }

        public int insertUserId { get; set; }

        public DateTime insertDate { get; set; }
    }
}
