//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace elbannaIcons.Models
//{
//    [Table("ic_item")] // لربط الكود بالجدول الفعلي في SQL
//    public class Category
//    {
//        [Key]
//        [Column("id")] // لربط المعرف بعمود id
//        public int Id { get; set; }

//        [Required(ErrorMessage = "اسم الصنف مطلوب")]
//        [Column("item")] // لربط الاسم بعمود item في SQL
//        [Display(Name = "الصنف")]
//        public string Name { get; set; }

//        [NotMapped] // لأن عمود "موقوف" غير موجود حالياً في قاعدة بياناتك
//        public bool IsStopped { get; set; }
//    }
//}