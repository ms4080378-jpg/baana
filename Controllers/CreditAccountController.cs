using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace elbanna.Controllers
{
    public class CreditAccountController : Controller
    {
        private readonly AppDbContext _context;
        private const string SCREEN_NAME = "إعدادات العهدة";

        public CreditAccountController(AppDbContext context)
        {
            _context = context;
        }

        // فتح نافذة العهدة
        public IActionResult Popup()
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.CanAdd = PermissionHelper.Can(screenId, "Add", HttpContext);
            ViewBag.CanPrint = PermissionHelper.Can(screenId, "Print", HttpContext);

            return View();
        }

        // حفظ عهدة جديدة
        [HttpPost]
        public IActionResult Save([FromBody] acc_CreditAccount model)
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");
            if (!PermissionHelper.Can(screenId, "Add", HttpContext))
                return Forbid("غير مسموح لك بالحفظ");

            if (string.IsNullOrWhiteSpace(model.creditAcc))
                return BadRequest("الاسم مطلوب");

            _context.acc_CreditAccounts.Add(model);
            _context.SaveChanges();

            return Ok();
        }

        // عرض القائمة
        [HttpGet]
        public IActionResult GetList(string q = "")
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");

            // عرض القائمة نربطها بـ Print
            if (!PermissionHelper.Can(screenId, "Print", HttpContext) &&
                !PermissionHelper.Can(screenId, "Add", HttpContext) &&
                !PermissionHelper.Can(screenId, "Edit", HttpContext) &&
                !PermissionHelper.Can(screenId, "Delete", HttpContext))
                return Forbid("غير مسموح بعرض القائمة");

            var data = _context.acc_CreditAccounts
                .Where(x => q == "" || x.creditAcc.Contains(q))
                .OrderBy(x => x.creditAcc)
                .Select(x => new
                {
                    x.id,
                    x.creditAcc
                })
                .ToList();

            return Json(data);
        }
    }
}
