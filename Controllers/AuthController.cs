using elbanna.Data;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace elbanna.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // =======================
        // GET : Login
        // =======================
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Home");

            return View(new LoginVM());
        }

        // =======================
        // POST : Login
        // =======================
        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            var username = model.Username?.Trim();
            var password = model.Password?.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                model.ErrorMessage = "من فضلك أدخل اسم المستخدم وكلمة السر";
                return View(model);
            }

            // =======================
            // التحقق من المستخدم
            // =======================
            var user = _context.hr_user
                .FirstOrDefault(u => u.username == username);

            if (user == null)
            {
                model.ErrorMessage = "راجع اسم المستخدم أو كلمة السر";
                return View(model);
            }

            // =======================
            // 🔐 التحقق من كلمة السر
            // بدون أي تعديل في DB
            // =======================

            if (!string.IsNullOrEmpty(user.password) && user.password.StartsWith("$2"))
            {
                // حالة BCrypt (لو موجودة قديمًا)
                if (!BCrypt.Net.BCrypt.Verify(password, user.password))
                {
                    model.ErrorMessage = "راجع اسم المستخدم أو كلمة السر";
                    return View(model);
                }
            }
            else
            {
                // حالة Plain Text
                if (user.password != password)
                {
                    model.ErrorMessage = "راجع اسم المستخدم أو كلمة السر";
                    return View(model);
                }
            }

            // =======================
            // منع الدخول مرتين (زي الديسك توب)
            // =======================
            if (user.islogged == true && user.username != "fathi")
            {
                // لو Session قديمة أو السيرفر اتعمله Restart
                user.islogged = false;
            }

            // =======================
            // تحميل بيانات المستخدم في Session
            // =======================
            HttpContext.Session.SetInt32("UserId", user.id);
            HttpContext.Session.SetString("Username", user.username ?? "");
            HttpContext.Session.SetString("UserJob", user.job ?? "");

            HttpContext.Session.SetInt32("AllowShowDays", user.allowShowDays ?? 0);
            HttpContext.Session.SetInt32("AllowSaveDays", user.allowSaveDays ?? 0);
            HttpContext.Session.SetInt32("AllowFutureSaveDays", user.allowFutureSaveDays ?? 0);
            HttpContext.Session.SetInt32(
                "AllowShowOtherData",
                user.allowShowOtherData == true ? 1 : 0
            );

            HttpContext.Session.SetInt32("CanReview", user.canReview == true ? 1 : 0);
            HttpContext.Session.SetInt32("CanPaid", user.canPaid == true ? 1 : 0);
            HttpContext.Session.SetInt32(
                "CanUpdateCustody",
                user.canUpdateCustody == true ? 1 : 0
            );

            // =======================
            // صلاحيات الشاشات
            // =======================
            var screenPermissions = _context.st_userPermission
                .Where(p => p.userId == user.id)
                .ToList();

            HttpContext.Session.SetString(
                "permissions",
                JsonConvert.SerializeObject(screenPermissions)
            );

            // =======================
            // صلاحيات المواقع (Cost Centers)
            // =======================
            var ccPermissions = _context.st_UserCCPermission
     .Where(x => x.userId == user.id)
     .Select(x => new
     {
         x.costcenter,
         x.canAdd,
         x.canEdit,
         x.canDelete,
         x.canPrint
     })
     .ToList();

            HttpContext.Session.SetString(
                "cc_permissions",
                JsonConvert.SerializeObject(ccPermissions)
            );






            // =======================
            // تسجيل الدخول
            // =======================
            user.islogged = true;
            user.lastLogin = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }



        // =======================
        // Logout
        // =======================
        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                var user = _context.hr_user.Find(userId);
                if (user != null)
                {
                    user.islogged = false;
                    _context.SaveChanges();
                }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // =======================
        // Access Denied
        // =======================
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
