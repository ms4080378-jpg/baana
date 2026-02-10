using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class ChequeRecController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.ChequeRec;

        public ChequeRecController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public IActionResult Index()
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var allCC = _context.acc_CostCenters.AsNoTracking().ToList();
            var allowedCC = allCC
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .ToList();

            var vm = new ChequeRecVM
            {
                DateFrom = DateTime.Today,
                DateTo = DateTime.Today,
                ProcessDate = DateTime.Today,

                CostCenters = allowedCC,
                Dealers = _context.acc_Dealers.AsNoTracking().ToList(),
                Banks = _context.acc_Dealers.Where(x => x.isBank).AsNoTracking().ToList(),
                CreditAccounts = _context.acc_CreditAccounts.AsNoTracking().ToList()
            };

            return View(vm);
        }

        // =========================
        // LOAD LIST
        // =========================
        [HttpPost]
        public IActionResult LoadList([FromBody] ChequeRecVM model)
        {
            if (model?.CostCenterId == null)
                return Content("");

            // ❌ موقع غير مسموح
            if (!PermissionHelper.CanCostCenter(model.CostCenterId.Value, HttpContext))
                return Content("");

            var fromDate = model.DateFrom.Date;
            var toDate = model.DateTo.Date.AddDays(1);

            var list = _context.acc_ChequeRecs
                .AsNoTracking()
                .Where(x =>
                    x.processDate != null &&
                    x.processDate >= fromDate &&
                    x.processDate < toDate &&
                    x.costcenterId == model.CostCenterId
                )
                .OrderByDescending(x => x.id)
                .Take(500)
                .ToList();

            return PartialView("_ChequeRecTable", list);
        }

        // =========================
        // SAVE (ADD)
        // =========================
        [HttpPost]
        public IActionResult Save([FromBody] ChequeRecVM model)
        {
            if (model == null)
                return BadRequest("بيانات غير صالحة");

            if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                return Forbid("غير مسموح لك بالحفظ");

            if (model.CostCenterId == null)
                return BadRequest("الموقع مطلوب");

            if (!PermissionHelper.CanCostCenter(model.CostCenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            if (model.DealerId == null || model.DealerId == 0)
                return BadRequest("البيان مطلوب");

            // تأمين FK
            int? safeDealerId = _context.acc_Dealer.Any(x => x.id == model.DealerId) ? model.DealerId : null;
            int? safeBankId = (model.BankId != null && _context.acc_Dealer.Any(x => x.id == model.BankId)) ? model.BankId : null;

            // المستخدم من السيشن
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int? safeUserId = _context.hr_user.Any(x => x.id == userId) ? userId : (int?)null;

            var costCenterName = _context.acc_CostCenters
                .Where(x => x.id == model.CostCenterId.Value)
                .Select(x => x.costCenter)
                .FirstOrDefault();

            var rec = new acc_ChequeRec
            {
                processDate = model.ProcessDate,
                costcenterId = model.CostCenterId,
                costcenter = costCenterName,

                dealerId = safeDealerId,
                bankId = safeBankId,

                chequeNo = model.ChequeNo,
                invoiceCode = model.InvoiceCode,
                total = model.Total,
                notes = model.Notes,

                fromAcc = model.FromAcc,
                toAcc = model.ToAcc,

                insertDate = DateTime.Now,
                insertUserId = safeUserId,

                isReviewed = false,
                isPaid = false
            };

            _context.acc_ChequeRecs.Add(rec);
            _context.SaveChanges();

            return Json(true);
        }

        // =========================
        // UPDATE (EDIT)
        // =========================
        [HttpPost]
        public IActionResult Update([FromBody] ChequeRecVM model)
        {
            if (model == null || model.Id == 0)
                return BadRequest("اختر صف للتعديل");

            if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                return Forbid("غير مسموح لك بالتعديل");

            var rec = _context.acc_ChequeRecs.FirstOrDefault(x => x.id == model.Id);
            if (rec == null)
                return BadRequest("السجل غير موجود");

            // ❌ موقع السجل لازم يكون مسموح
            if (!rec.costcenterId.HasValue || !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // ❌ ممنوع التعديل بعد المراجعة
            if (rec.isReviewed == true)
                return Forbid("لا يمكن التعديل بعد المراجعة");

            if (model.CostCenterId == null)
                return BadRequest("الموقع مطلوب");

            if (!PermissionHelper.CanCostCenter(model.CostCenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            var costCenter = _context.acc_CostCenters.FirstOrDefault(x => x.id == model.CostCenterId.Value);
            if (costCenter == null)
                return BadRequest("الموقع غير موجود");

            int? safeDealerId = (model.DealerId != null && _context.acc_Dealer.Any(x => x.id == model.DealerId)) ? model.DealerId : null;
            int? safeBankId = (model.BankId != null && _context.acc_Dealer.Any(x => x.id == model.BankId)) ? model.BankId : null;

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int? safeUserId = _context.hr_user.Any(x => x.id == userId) ? userId : (int?)null;

            rec.processDate = model.ProcessDate;
            rec.costcenterId = costCenter.id;
            rec.costcenter = costCenter.costCenter;

            rec.dealerId = safeDealerId;
            rec.bankId = safeBankId;

            rec.chequeNo = model.ChequeNo;
            rec.invoiceCode = model.InvoiceCode;
            rec.total = model.Total;
            rec.notes = model.Notes;
            rec.fromAcc = model.FromAcc;
            rec.toAcc = model.ToAcc;

            rec.lastUpdateDate = DateTime.Now;
            rec.lastUpdateUserId = safeUserId;

            _context.SaveChanges();
            return Json(true);
        }

        // =========================
        // DELETE
        // =========================
        [HttpPost]
        public IActionResult Delete([FromBody] int id)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح لك بالحذف");

            var rec = _context.acc_ChequeRecs.Find(id);
            if (rec == null)
                return BadRequest("السجل غير موجود");

            // ❌ موقع غير مسموح
            if (!rec.costcenterId.HasValue || !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // (اختياري) منع الحذف بعد المراجعة
            // if (rec.isReviewed == true) return Forbid("لا يمكن الحذف بعد المراجعة");

            var dailies = _context.acc_Dailies
                .Where(x => x.invoiceCode == rec.invoiceCode && x.costcenterId == rec.costcenterId);

            _context.acc_Dailies.RemoveRange(dailies);
            _context.acc_ChequeRecs.Remove(rec);
            _context.SaveChanges();

            return Content("تم الحذف");
        }

        // =========================
        // REVIEW
        // =========================
        public class ToggleReviewVM { public int id { get; set; } }

        [HttpPost]
        public IActionResult ToggleReview([FromBody] ToggleReviewVM model)
        {
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var rec = _context.acc_ChequeRecs.FirstOrDefault(x => x.id == model.id);
            if (rec == null)
                return BadRequest("السجل غير موجود");

            if (!rec.costcenterId.HasValue ||
                !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            rec.isReviewed = !rec.isReviewed;
            _context.SaveChanges();

            return Json(rec.isReviewed);
        }


        // =========================
        // PAID
        // =========================
        public class TogglePaidVM { public int id { get; set; } }

        [HttpPost]
        public IActionResult TogglePaid([FromBody] TogglePaidVM model)
        {
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var rec = _context.acc_ChequeRecs.FirstOrDefault(x => x.id == model.id);
            if (rec == null)
                return BadRequest("السجل غير موجود");

            if (!rec.costcenterId.HasValue ||
                !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            rec.isPaid = !rec.isPaid;
            _context.SaveChanges();

            return Json(rec.isPaid);
        }

    }
}
