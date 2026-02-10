using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("hr_user")]
    public class hr_user
    {
        [Key]
        public int id { get; set; }

        public string username { get; set; }
        public string password { get; set; }
        public string job { get; set; }

        public DateTime? lastLogin { get; set; }

        public int? jobId { get; set; }
        public int? allowSaveDays { get; set; }
        public int? allowShowDays { get; set; }

        public bool? allowShowOtherData { get; set; }
        public int? allowFutureSaveDays { get; set; }

        public bool? islogged { get; set; }
        public bool? canReview { get; set; }
        public bool? canPaid { get; set; }
        public bool? canUpdateCustody { get; set; }
    }
}
