using System.ComponentModel.DataAnnotations;

namespace elbanna.ViewModels
{
    public class UserFormVM
    {
        public int id { get; set; }

        // ====== بيانات المستخدم ======
        [Required(ErrorMessage = "يجب إدخال اسم المستخدم")]
        public string username { get; set; } = "";

        // ✅ خليك على Password فقط (بدون password ثانية)
        // لو عايزها إجبارية دائمًا زي الديسكتوب:
        [Required(ErrorMessage = "يجب إدخال كلمة السر")]
        public string Password { get; set; } = "";

        public bool islogged { get; set; }

        // ✅ الوظيفة تعتمد على jobId فقط
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار الوظيفة")]
        public int jobId { get; set; }

        // ====== بيانات أخرى ======
        public int allowSaveDays { get; set; }
        public int allowFutureSaveDays { get; set; }
        public int allowShowDays { get; set; }

        public bool allowShowOtherData { get; set; }
        public bool canReview { get; set; }
        public bool canPaid { get; set; }
        public bool canUpdateCustody { get; set; }
    }
}
