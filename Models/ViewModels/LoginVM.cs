
using System.ComponentModel.DataAnnotations;

namespace elbanna.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; }

        [Required(ErrorMessage = "كلمة السر مطلوبة")]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }
    }
}
