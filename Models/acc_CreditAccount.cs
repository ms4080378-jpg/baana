using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_CreditAccount")]
    public class acc_CreditAccount
    {
        [Key]
        public int id { get; set; }

        [Column("creditAcc")]
        public string creditAcc { get; set; }
    }
}
