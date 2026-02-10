using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elbanna.Models
{
    [Table("IC_ItemStore")]
    public class IC_ItemStore
    {
        [Key]
        public int id { get; set; }
        public string? item { get; set; }
        public bool? isStopped { get; set; }   // ✅ أضف السطر ده
        public bool? isVendorItem { get; set; }

    }

}
