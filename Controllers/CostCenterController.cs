using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using Microsoft.AspNetCore.Mvc;

namespace elbanna.Controllers
{
    public class CostCenterController : Controller
    {
        private readonly AppDbContext _context;
        private const string SCREEN_NAME = "إعدادات المواقع";

        public CostCenterController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);

            // منع فتح الشاشة لو مفيش أي صلاحية
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            // صلاحيات الأزرار للـ View
            ViewBag.CanEdit = PermissionHelper.Can(screenId, "Edit", HttpContext) ||
                              PermissionHelper.Can(screenId, "Add", HttpContext);

            return View(_context.acc_CostCenter.ToList());
        }

        [HttpPost]
        public IActionResult Save(List<acc_CostCenter> model)
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);

            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");

            // حفظ إعدادات المواقع = تعديل
            if (!PermissionHelper.Can(screenId, "Edit", HttpContext) &&
                !PermissionHelper.Can(screenId, "Add", HttpContext))
                return Forbid("غير مسموح لك بالحفظ");

            foreach (var row in model)
            {
                if (row.id == 0)
                    _context.acc_CostCenter.Add(row);
                else
                    _context.acc_CostCenter.Update(row);
            }

            _context.SaveChanges();
            return Ok();
        }
    }
}
