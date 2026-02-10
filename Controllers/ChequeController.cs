using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.Models.ViewModels.elbanna.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class ChequeController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.Cheque;

        public ChequeController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Index
        public IActionResult Index()
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Banks = _context.acc_Dealers
                .Where(x => x.isBank && !x.isStopped)
                .AsNoTracking()
                .ToList();

            return View();
        }

        // 🔹 Load (عرض القائمة)
        [HttpGet]
        public IActionResult Load()
        {
            // ❌ منع فتح الشاشة / أو على الأقل منع المستخدم اللي ملوش Open
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            var data = (
                from c in _context.st_Cheques
                join d in _context.acc_Dealers on c.dealerId equals d.id
                orderby c.id descending
                select new ChequeVM
                {
                    id = c.id,
                    chequeNo = c.chequeNo,
                    qty = c.qty,
                    dealerId = c.dealerId,
                    dealer = d.dealer,
                    notes = c.notes,
                    respons = c.respons
                }
            ).AsNoTracking()
             .Take(500)
             .ToList();

            return Json(data);
        }

        // 🔹 Save (Add / Edit)
        [HttpPost]
        public IActionResult Save([FromBody] st_Cheque model)
        {
            if (model == null)
                return BadRequest("بيانات غير صالحة");

            // ✅ صلاحيات Add/Edit
            if (model.id == 0)
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                    return Forbid("غير مسموح لك بالحفظ");
            }
            else
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                    return Forbid("غير مسموح لك بالتعديل");
            }

            if (model.dealerId == null || model.dealerId <= 0)
                return BadRequest("يجب اختيار البنك");

            if (model.qty <= 0)
                return BadRequest("يجب اختيار الكمية");

            if (model.chequeNo <= 0)
                return BadRequest("يجب اختيار رقم أول ورقة");

            // 🔴 منع تداخل دفاتر الشيكات (زي الديسك توب)
            bool exists = _context.st_Cheques.Any(x =>
                x.dealerId == model.dealerId &&
                x.id != model.id &&
                (
                    model.chequeNo >= x.chequeNo &&
                    model.chequeNo <= x.chequeNo + x.qty
                )
            );

            if (exists)
                return BadRequest("هذا الدفتر موجود مسبقًا");

            // ✅ userId من السيشن (زي Daily / ChequeRec)
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (model.id == 0)
            {
                model.insertDate = DateTime.Now;
                model.insertUserId = userId == 0 ? null : userId; // لو عندك insertUserId nullable

                _context.st_Cheques.Add(model);
            }
            else
            {
                var row = _context.st_Cheques.FirstOrDefault(x => x.id == model.id);
                if (row == null) return NotFound("السجل غير موجود");

                row.qty = model.qty;
                row.notes = model.notes;
                row.respons = model.respons;
                row.dealerId = model.dealerId;

                row.lastUpdateDate = DateTime.Now;
                row.lastUpdateUserId = userId == 0 ? null : userId; // لو nullable
            }

            _context.SaveChanges();
            return Json(true);
        }
    }
}
