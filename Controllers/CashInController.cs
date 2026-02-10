using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class CashInController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.CashIn;

        public CashInController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // فتح الشاشة
        // =========================
        public IActionResult Index()
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var allCostCenters = _context.acc_CostCenter
    .AsNoTracking()
    .OrderBy(x => x.costCenter)
    .ToList();   // 👈 تحميل من DB فقط

            ViewBag.CostCenters = allCostCenters
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .ToList();   // 👈 فلترة في الذاكرة


            ViewBag.Names = _context.acc_incomecash
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.payer))
                .Select(x => x.payer)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return View();
        }

        // =========================
        // عرض القائمة
        // =========================
        public IActionResult List(DateTime date, int costCenterId)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var allowOther =
                HttpContext.Session.GetInt32("AllowShowOtherData") == 1;

            // =========================
            // تحديد المواقع المسموح بها
            // =========================
            List<int> allowedCostCenters;

            if (costCenterId == 0)
            {
                // 👈 كل المواقع المسموح بها
                allowedCostCenters = PermissionHelper
                    .GetAllowedCostCenters(HttpContext);
            }
            else
            {
                // 👈 موقع واحد
                if (!PermissionHelper.CanCostCenter(costCenterId, HttpContext))
                    return Forbid();

                allowedCostCenters = new List<int> { costCenterId };
            }
            var data = _context.acc_incomecash
                .AsNoTracking()
                .Where(x =>
                    x.date.HasValue &&
                    x.date.Value.Date == date.Date &&
                    x.costcenterId.HasValue &&
                    allowedCostCenters.Contains(x.costcenterId.Value) &&
                    (
                        allowOther ||
                        x.insertUserId == userId ||
                        x.lastUpdateUserId == userId
                    )
                )
                .Select(x => new
                {
                    x.id,
                    custody = x.payer ?? "",
                    balance = x.balance ?? 0,
                    date = x.date,
                    notes = x.notes ?? "",
                    userName = _context.hr_user
                        .Where(u => u.id == x.insertUserId)
                        .Select(u => u.username)
                        .FirstOrDefault() ?? "",
                    siteId = x.costcenterId,
                    siteName = x.costcenter,
                    reviewed = x.isReviewed ?? false
                })
                .ToList();


            return Json(data);
        }


        // =========================
        // حفظ (إضافة / تعديل)
        // =========================
        [HttpPost]
        public IActionResult Save([FromBody] CashInVM model)
        {
            if (model == null)
                return BadRequest("بيانات غير صحيحة");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // =========================
            // 🔐 صلاحية Add / Edit (Screen)
            // =========================
            if (model.Id == 0)
            {
                if (!PermissionViewHelper.CanAdd(HttpContext, SCREEN_ID))
                    return Forbid("غير مسموح لك بالإضافة");
            }
            else
            {
                if (!PermissionViewHelper.CanEdit(HttpContext, SCREEN_ID))
                    return Forbid("غير مسموح لك بالتعديل");
            }

            // =========================
            // 🔐 صلاحية الموقع
            // =========================
            if (!model.CostCenterId.HasValue ||
                !PermissionHelper.CanCostCenter(model.CostCenterId.Value, HttpContext))
                return Forbid("غير مسموح على هذا الموقع");

            acc_incomecash row;

            // =========================
            // ✏️ تعديل
            // =========================
            if (model.Id > 0)
            {
                row = _context.acc_incomecash.Find(model.Id);
                if (row == null)
                    return NotFound();

                // ❌ ممنوع التعديل بعد المراجعة (إلا فتحي)
                if (row.isReviewed == true &&
                    !PermissionViewHelper.IsFathi(HttpContext))
                    return Forbid("لا يمكن التعديل بعد المراجعة");

                row.lastUpdateDate = DateTime.Now;
                row.lastUpdateUserId = userId;
            }
            // =========================
            // ➕ إضافة
            // =========================
            else
            {
                row = new acc_incomecash
                {
                    insertDate = DateTime.Now,
                    insertUserId = userId,
                    isReviewed = false
                };

                _context.acc_incomecash.Add(row);
            }

            // =========================
            // 💾 حفظ البيانات
            // =========================
            var cc = _context.acc_CostCenter
                .First(x => x.id == model.CostCenterId.Value);

            row.date = model.Date;
            row.payer = model.Custody?.Trim();
            row.costcenterId = cc.id;
            row.costcenter = cc.costCenter;
            row.balance = model.Balance;
            row.notes = model.Notes?.Trim();

            _context.SaveChanges();

            return Ok(new
            {
                id = row.id,
                reviewed = row.isReviewed
            });
        }


        // =========================
        // حذف
        // =========================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح لك بالحذف");

            var row = _context.acc_incomecash.Find(id);
            if (row == null)
                return NotFound();

            // ❌ ممنوع الحذف بعد المراجعة
            if (!PermissionViewHelper.CanDelete(HttpContext, SCREEN_ID))
                return Forbid();

            if (row.isReviewed == true &&
                !PermissionViewHelper.IsFathi(HttpContext))
                return Forbid("لا يمكن الحذف بعد المراجعة");


            _context.acc_incomecash.Remove(row);
            _context.SaveChanges();

            return Ok();
        }

        // =========================
        // مراجعة / إلغاء مراجعة
        // =========================
        [HttpPost]
        public IActionResult ToggleReview([FromBody] ReviewVM model)
        {
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var row = _context.acc_incomecash.Find(model.Id);
            if (row == null)
                return NotFound();

            row.isReviewed = model.Review;
            _context.SaveChanges();

            return Ok();
        }



        public class ReviewVM
        {
            public int Id { get; set; }
            public bool Review { get; set; }
        }
    }
}
