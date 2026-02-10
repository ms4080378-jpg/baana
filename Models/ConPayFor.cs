using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("con_payFor")]
    public class ConPayFor
    {
        public int id { get; set; }
        public string payFor { get; set; }
        public string factor { get; set; }
        public decimal factorValue { get; set; }
    }

}