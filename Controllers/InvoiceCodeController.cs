using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace elbanna.Controllers
{
    public class InvoiceCodeController : Controller
    {
        private readonly AppDbContext _context;
        private const string SCREEN_NAME = "إعدادات الإيصالات";

        public InvoiceCodeController(AppDbContext context)
        {
            _context = context;
        }

        // فتح الشاشة
        public IActionResult Index()
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.CanAdd = PermissionHelper.Can(screenId, "Add", HttpContext);
            ViewBag.CanEdit = PermissionHelper.Can(screenId, "Edit", HttpContext) || PermissionHelper.Can(screenId, "Add", HttpContext);
            ViewBag.CanPrint = PermissionHelper.Can(screenId, "Print", HttpContext);

            return View();
        }

        // عرض القائمة
        [HttpGet]
        public IActionResult GetList()
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");
            if (!PermissionHelper.Can(screenId, "Print", HttpContext) &&
                !PermissionHelper.Can(screenId, "Add", HttpContext) &&
                !PermissionHelper.Can(screenId, "Edit", HttpContext) &&
                !PermissionHelper.Can(screenId, "Delete", HttpContext))
                return Forbid("غير مسموح بعرض القائمة");

            var data =
                from i in _context.acc_invoiceCode
                join d in _context.acc_Dealer
                    on i.invoiceType equals d.id.ToString()
                orderby i.id
                select new
                {
                    i.id,
                    invoiceCode = i.invoiceCode,   // رقم الإيصال
                    invoiceType = d.dealer         // اسم المتعامل (البيان)
                };

            return Json(data.ToList());
        }





        // حفظ
        [HttpPost]
        public IActionResult Save([FromBody] acc_invoiceCode model)
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");

            bool canAdd = PermissionHelper.Can(screenId, "Add", HttpContext);
            bool canEdit = PermissionHelper.Can(screenId, "Edit", HttpContext);

            if (model.id == 0)
            {
                if (!canAdd) return Forbid("غير مسموح لك بالحفظ");
            }
            else
            {
                if (!canEdit && !canAdd) return Forbid("غير مسموح لك بالتعديل");
            }

            if (string.IsNullOrWhiteSpace(model.invoiceCode))
                return BadRequest("البيان مطلوب");

            if (model.id == 0)
                _context.acc_invoiceCode.Add(model);
            else
                _context.acc_invoiceCode.Update(model);

            _context.SaveChanges();
            return Ok();


        }

        [HttpPost]
        public IActionResult SaveAll([FromBody] List<acc_invoiceCode> list)
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");
            if (!PermissionHelper.Can(screenId, "Edit", HttpContext) && !PermissionHelper.Can(screenId, "Add", HttpContext))
                return Forbid("غير مسموح لك بالحفظ");

            foreach (var row in list)
            {
                if (row.id == 0)
                    _context.acc_invoiceCode.Add(row);
                else
                    _context.acc_invoiceCode.Update(row);
            }

            _context.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public IActionResult SaveBatch([FromBody] List<acc_invoiceCode> list)
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");
            if (!PermissionHelper.Can(screenId, "Edit", HttpContext) && !PermissionHelper.Can(screenId, "Add", HttpContext))
                return Forbid("غير مسموح لك بالحفظ");

            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.invoiceCode))
                    continue;

                if (item.id == 0)
                    _context.acc_invoiceCode.Add(item);
                else
                    _context.acc_invoiceCode.Update(item);
            }

            _context.SaveChanges();
            return Ok();
        }

    }
}
