using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("acc_invoiceCode")]
    public class acc_invoiceCode
    {
        public int id { get; set; }
        public string invoiceCode { get; set; }
        public string invoiceType { get; set; }
    }

}
