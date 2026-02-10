using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class BankTransferController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.BankTransfer;

        public BankTransferController(AppDbContext context)
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

            var vm = new BankTransferVM
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
        public IActionResult LoadList([FromBody] BankTransferVM model)
        {
            if (model?.CostCenterId == null)
                return Content("");

            // ❌ موقع غير مسموح
            if (!PermissionHelper.CanCostCenter(model.CostCenterId.Value, HttpContext))
                return Content("");

            var fromDate = model.DateFrom.Date;
            var toDate = model.DateTo.Date.AddDays(1);

            var list = _context.acc_BankTransfers
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

            return PartialView("_BankTransferTable", list);
        }

        // =========================
        // SAVE (ADD / EDIT)
        // =========================
        [HttpPost]
        public IActionResult Save([FromBody] BankTransferVM model)
        {
            if (model == null)
                return BadRequest("بيانات غير صالحة");

            if (model.CostCenterId == null)
                return BadRequest("الموقع مطلوب");

            // ❌ موقع غير مسموح
            if (!PermissionHelper.CanCostCenter(model.CostCenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // ✅ صلاحيات Add/Edit
            if (model.Id == 0)
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                    return Forbid("غير مسموح لك بالحفظ");
            }
            else
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                    return Forbid("غير مسموح لك بالتعديل");
            }

            var costCenter = _context.acc_CostCenters
                .FirstOrDefault(x => x.id == model.CostCenterId.Value);

            if (costCenter == null)
                return BadRequest("الموقع غير موجود");

            // userId من السيشن
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int? safeUserId = _context.hr_user.Any(x => x.id == userId) ? userId : (int?)null;

            acc_BankTransfer rec;

            // ✳️ تعديل
            if (model.Id > 0)
            {
                rec = _context.acc_BankTransfers.FirstOrDefault(x => x.id == model.Id);
                if (rec == null)
                    return BadRequest("السجل غير موجود");

                // ❌ ممنوع تعديل بعد المراجعة (زي Daily)
                if (rec.isReviewed == true)
                    return Forbid("لا يمكن التعديل بعد المراجعة");

                // ❌ تأمين موقع السجل الحالي
                if (!rec.costcenterId.HasValue || !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                    return Forbid("غير مسموح بالموقع");

                rec.lastUpdateDate = DateTime.Now;
                rec.lastUpdateUserId = safeUserId;
            }
            // ➕ إضافة
            else
            {
                rec = new acc_BankTransfer
                {
                    insertDate = DateTime.Now,
                    insertUserId = safeUserId,
                    isReviewed = false
                };
                _context.acc_BankTransfers.Add(rec);
            }

            rec.processDate = model.ProcessDate;
            rec.costcenterId = costCenter.id;
            rec.costcenter = costCenter.costCenter;

            rec.total = model.Total;
            rec.notes = model.Notes;
            rec.fromAcc = model.FromAcc;
            rec.toAcc = model.ToAcc;
            rec.invoiceCode = model.InvoiceCode;

            // لو عندك bankId في الجدول
            rec.bankId = model.BankId;

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

            var rec = _context.acc_BankTransfers.Find(id);
            if (rec == null)
                return BadRequest("السجل غير موجود");

            // ❌ موقع غير مسموح
            if (!rec.costcenterId.HasValue || !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            _context.acc_BankTransfers.Remove(rec);
            _context.SaveChanges();

            return Content("تم الحذف");
        }

        // =========================
        // REVIEW (يتوافق مع JS الحالي)
        // =========================
        public class ReviewVM
        {
            public int id { get; set; }
            public bool reviewed { get; set; }
        }

        [HttpPost]
        public IActionResult Review([FromBody] ReviewVM model)
        {
            // ✅ مراجع أو فتحي فقط
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var rec = _context.acc_BankTransfers.FirstOrDefault(x => x.id == model.id);
            if (rec == null)
                return BadRequest("السجل غير موجود");

            if (!rec.costcenterId.HasValue ||
                !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            rec.isReviewed = model.reviewed;
            _context.SaveChanges();

            return Json(true);
        }

    }
}
