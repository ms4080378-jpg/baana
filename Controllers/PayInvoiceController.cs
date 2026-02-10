using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{

    public class PayInvoiceController : Controller
    {
        private readonly AppDbContext _context;

        private const int SCREEN_ID = (int)Screens.PayInvoice;

        int UserId =>
            HttpContext.Session.GetInt32("UserId") ?? 0;

        bool AllowOther =>
            HttpContext.Session.GetInt32("AllowShowOtherData") == 1;

        bool CanReview => PermissionViewHelper.CanReview(HttpContext);

        public PayInvoiceController(AppDbContext context)
        {
            _context = context;
        }

        // =============================
        // Index
        // =============================

        public IActionResult Index()
        {
            // 🔒 منع فتح الشاشة لو المستخدم مالوش صلاحية عليها
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var allCostCenters = _context.acc_CostCenters
                .AsNoTracking()
                .OrderBy(cc => cc.costCenter)
                .ToList();

            var allowedCostCenters = allCostCenters
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .ToList();

            var vm = new PayInvoiceVM
            {
                Date = DateTime.Now,
                CostCenters = allowedCostCenters,
                CanReview = PermissionViewHelper.CanReview(HttpContext),
                CanPaid = PermissionViewHelper.CanReview(HttpContext)
            };

            return View(vm);
        }









        // =============================
        // Show List
        // =============================
        [HttpPost]
        public IActionResult ShowList(DateTime date, int costCenterId)
        {
            try
            {
                var allowedIds = PermissionHelper.GetAllowedCostCenters(HttpContext);

                // لو المستخدم مالوش أي مواقع مسموحة → رجّع فاضي
                if (allowedIds.Count == 0)
                    return Json(new { ok = true, data = new List<con_payInvoice>() });

                // لو اختار موقع محدد لازم يكون مسموح
                if (costCenterId > 0 && !allowedIds.Contains(costCenterId))
                    return Json(new { ok = false, message = "غير مسموح لك بعرض هذا الموقع" });

                var query = _context.con_payInvoices
                    .AsNoTracking()
                    .Where(x =>
                        x.date.HasValue &&
                        x.date.Value.Date >= date.Date &&
                        (
                            // لو "الكل" اعرض المسموح فقط
                            (costCenterId == 0 && allowedIds.Contains(x.costcenterId)) ||
                            // لو موقع محدد
                            (costCenterId > 0 && x.costcenterId == costCenterId)
                        ) &&
                        (
                            AllowOther ||
                            x.insertUserId == UserId ||
                            x.lastUpdateUserId == UserId
                        )
                    )
                    .OrderByDescending(x => x.id)
                    .ToList();

                return Json(new { ok = true, data = query });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }





        // =============================
        // Load Invoices
        // =============================
        [HttpPost]
        public IActionResult LoadInvoices(int costCenterId)
        {
            if (costCenterId <= 0)
                return Json(new List<string>());

            if (!PermissionHelper.CanCostCenter(costCenterId, HttpContext))
                return Forbid();

            var invoices = _context.ConInvoices
                .Where(x => x.costcenterId == costCenterId)
                .GroupBy(x => x.invoiceCode)
                .Select(x => x.Key)
                .ToList();

            return Json(invoices);
        }

        [HttpPost]
        public IActionResult CalcInvoice(int costCenterId, string invoiceCode, int currentId)
        {
            if (costCenterId <= 0 || string.IsNullOrWhiteSpace(invoiceCode))
                return Json(new { total = 0, debit = 0, net = 0, paid = 0 });

            if (!PermissionHelper.CanCostCenter(costCenterId, HttpContext))
                return Forbid();

            var inv = _context.ConInvoices
                .Where(x => x.costcenterId == costCenterId && x.invoiceCode == invoiceCode);

            var total = inv.Sum(x => x.balance ?? 0);
            var debit = inv.Sum(x => x.net ?? 0);

            var paid = _context.con_payInvoices
                .Where(x => x.costcenterId == costCenterId && x.invoiceCode == invoiceCode && x.id != currentId)
                .Sum(x => x.paid ?? 0);

            return Json(new { total, debit, net = total - debit, paid });
        }

        // =============================
        // Save (Add / Edit)
        // =============================
        [HttpPost]
        public IActionResult Save(PayInvoiceVM vm)
        {
            if (vm == null)
                return BadRequest("بيانات غير صحيحة");

            // 🔐 صلاحيات (إضافة / تعديل)
            if (vm.Id == 0)
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                    return BadRequest("غير مسموح لك بالحفظ");

            }
            else
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                    return BadRequest("غير مسموح لك بالتعديل");

            }

            // 🔐 صلاحية الموقع
            if (!PermissionHelper.CanCostCenter(vm.CostCenterId, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // 🔎 Validations
            if (string.IsNullOrEmpty(vm.InvoiceCode))
                return BadRequest("رقم المستخلص مطلوب");

            if (vm.Amount <= 0)
                return BadRequest("المبلغ غير صحيح");

            con_payInvoice row;

            if (vm.Id == 0)
            {
                // ➕ إضافة
                row = new con_payInvoice
                {
                    insertDate = DateTime.Now,
                    insertUserId = UserId,
                    isReviewed = false,
                    isPaid = false
                };

                _context.con_payInvoices.Add(row);
            }
            else
            {
                // ✏️ تعديل
                row = _context.con_payInvoices.FirstOrDefault(x => x.id == vm.Id);
                if (row == null)
                    return NotFound();

                // ❌ منع التعديل بعد المراجعة
                if (row.isReviewed == true && !CanReview)
                    return Forbid("لا يمكن التعديل بعد المراجعة");

                row.lastUpdateDate = DateTime.Now;
                row.lastUpdateUserId = UserId;
            }

            // 📝 تعيين البيانات
            row.date = vm.Date;
            row.invoiceCode = vm.InvoiceCode;
            row.costcenterId = vm.CostCenterId;
            row.costcenter = vm.CostCenterName;
            row.paid = vm.Amount;
            row.notes = vm.Notes;

            _context.SaveChanges();

            return Ok();
        }


        // =============================
        // Delete
        // =============================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return BadRequest("غير مسموح لك بالحذف");


            var row = _context.con_payInvoices.Find(id);
            if (row == null) return NotFound();

            if (row.isReviewed == true && !CanReview)
                return Forbid("لا يمكن الحذف بعد المراجعة");

            _context.con_payInvoices.Remove(row);
            _context.SaveChanges();

            return Ok();
        }

        // =============================
        // Review
        // =============================
        [HttpPost]
        public IActionResult ToggleReview(int id)
        {
            // ✅ مراجع أو فتحي فقط
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var row = _context.con_payInvoices.Find(id);
            if (row == null) return NotFound();

            row.isReviewed = !row.isReviewed;
            row.lastUpdateDate = DateTime.Now;
            row.lastUpdateUserId = UserId;

            _context.SaveChanges();
            return Ok(row.isReviewed);
        }



        // =============================
        // Paid
        // =============================
        [HttpPost]
        public IActionResult TogglePaid(int id)
        {
            // ✅ مراجع أو فتحي فقط
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var row = _context.con_payInvoices.Find(id);
            if (row == null) return NotFound();

            row.isPaid = !row.isPaid;
            row.lastUpdateDate = DateTime.Now;
            row.lastUpdateUserId = UserId;

            _context.SaveChanges();
            return Ok(row.isPaid);
        }


    }
}
