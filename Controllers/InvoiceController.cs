using ClosedXML.Excel;
using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _db;

        // ✅ غيرها لو اسم الشاشة مختلف في enum Screens
        private const int SCREEN_ID = (int)Screens.Invoice;

        public InvoiceController(AppDbContext db)
        {
            _db = db;
        }

        // ===============================
        // INDEX
        // ===============================
        public IActionResult Index(
            InvoiceIndexVM vm,
            bool showList = false,
            int? Invoice_costcenterId = null,
            DateTime? Invoice_date = null,
            int? selectedId = null
        )
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            vm ??= new InvoiceIndexVM();
            vm.Invoice ??= new ConInvoice();

            // ✅ فلترة المواقع المسموح بها
            var allCC = _db.acc_CostCenter.AsNoTracking().ToList();
            var allowedCC = allCC
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .ToList();

            vm.CostCenters = allowedCC;

            // PayFors
            vm.PayFors = _db.ConPayFors.AsNoTracking()
                .Select(x => new PayForVM
                {
                    id = x.id,
                    payFor = x.payFor,
                    factor = x.factor,
                    factorValue = x.factorValue
                })
                .ToList();

            vm.SelectedId = selectedId;

            if (!vm.Invoice.date.HasValue)
                vm.Invoice.date = DateTime.Today;

            if (Invoice_costcenterId.HasValue)
                vm.Invoice.costcenterId = Invoice_costcenterId;

            if (Invoice_date.HasValue)
                vm.Invoice.date = Invoice_date;

            // لو المستخدم محاول يفتح على costcenter غير مسموح
            if (vm.Invoice.costcenterId.HasValue &&
                !PermissionHelper.CanCostCenter(vm.Invoice.costcenterId.Value, HttpContext))
            {
                vm.Invoice.costcenterId = null;
            }

            if (!showList || !vm.Invoice.costcenterId.HasValue)
            {
                vm.ShowList = false;
                vm.List = new List<ConInvoice>();
                return View(vm);
            }

            // ✅ تأمين الموقع
            if (!PermissionHelper.CanCostCenter(vm.Invoice.costcenterId.Value, HttpContext))
            {
                vm.ShowList = false;
                vm.List = new List<ConInvoice>();
                return View(vm);
            }

            vm.ShowList = true;

            var q = _db.ConInvoices.AsNoTracking()
                .Where(x => x.costcenterId == vm.Invoice.costcenterId);

            // (اختياري) فلتر بالـ date لو عايزه زي شاشة تانية
            // لو عايزها على نفس اليوم:
            // var day = vm.Invoice.date.Value.Date;
            // q = q.Where(x => x.date.HasValue && x.date.Value.Date == day);

            vm.List = q
                .OrderByDescending(x => x.date)
                .ThenBy(x => x.invoiceCode)
                .ThenBy(x => x.id)
                .ToList();

            return View(vm);
        }

        // ===============================
        // SAVE (ADD / EDIT)
        // ===============================
        [HttpPost]
        public IActionResult SaveInvoice(InvoiceIndexVM vm, bool showList = true)
        {
            if (vm?.Invoice == null)
                return BadRequest("بيانات غير صالحة");

            if (vm.Invoice.costcenterId == null)
                return BadRequest("الموقع مطلوب");

            // ❌ موقع غير مسموح
            if (!PermissionHelper.CanCostCenter(vm.Invoice.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // ✅ صلاحيات Add/Edit
            if (vm.Invoice.id == 0)
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                    return Forbid("غير مسموح لك بالحفظ");
            }
            else
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                    return Forbid("غير مسموح لك بالتعديل");
            }

            if (!vm.Invoice.date.HasValue)
                vm.Invoice.date = DateTime.Today;

            if (string.IsNullOrEmpty(vm.Invoice.invoiceCode))
                return BadRequest("رقم المستخلص مطلوب");

            // cost center name
            var costCenter = _db.acc_CostCenter.FirstOrDefault(x => x.id == vm.Invoice.costcenterId.Value);
            if (costCenter == null)
                return BadRequest("الموقع غير موجود");

            // اسم البيان + المعامل من جدول البيان
            if (vm.Invoice.payForId.HasValue)
            {
                var payFor = _db.ConPayFors.FirstOrDefault(x => x.id == vm.Invoice.payForId.Value);
                if (payFor != null)
                {
                    vm.Invoice.payFor = payFor.payFor;
                    vm.Invoice.factor = payFor.factor;
                }
            }

            // userId من السيشن
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var userName = HttpContext.Session.GetString("UserName");
            int? safeUserId = _db.hr_user.Any(x => x.id == userId) ? userId : (int?)null;

            // duplicate check: نفس رقم المستخلص + نفس البيان (مع استثناء نفس السجل)
            bool duplicate = _db.ConInvoices.Any(x =>
                x.id != vm.Invoice.id &&
                x.costcenterId == vm.Invoice.costcenterId &&
                x.invoiceCode == vm.Invoice.invoiceCode &&
                x.payForId == vm.Invoice.payForId
            );
            if (duplicate)
                return BadRequest("هناك مستخلص بنفس الرقم");

            // Add/Edit
            if (vm.Invoice.id == 0)
            {
                vm.Invoice.insertDate = DateTime.Now;
                vm.Invoice.insertUserId = safeUserId;
                vm.Invoice.isReviewed = false;

                vm.Invoice.costcenter = costCenter.costCenter;
                vm.Invoice.costcenterId = costCenter.id;

                _db.ConInvoices.Add(vm.Invoice);
                _db.SaveChanges();
                vm.Invoice.id = vm.Invoice.id; // ensures id is populated
            }
            else
            {
                var rec = _db.ConInvoices.FirstOrDefault(x => x.id == vm.Invoice.id);
                if (rec == null)
                    return BadRequest("السجل غير موجود");

                // ❌ تأمين موقع السجل الحالي
                if (!rec.costcenterId.HasValue || !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                    return Forbid("غير مسموح بالموقع");

                // ❌ ممنوع تعديل بعد المراجعة
                if (rec.isReviewed == true && userName != "fathi")
                    return Forbid("لا يمكن التعديل بعد المراجعة (يسمح فقط للمدير)");

                rec.lastUpdateDate = DateTime.Now;
                rec.lastUpdateUserId = safeUserId;

                rec.date = vm.Invoice.date;
                rec.invoiceCode = vm.Invoice.invoiceCode;
                rec.invoiceType = vm.Invoice.invoiceType;

                rec.costcenterId = costCenter.id;
                rec.costcenter = costCenter.costCenter;

                rec.balance = vm.Invoice.balance;
                rec.factor = vm.Invoice.factor;
                rec.factorValue = vm.Invoice.factorValue;
                rec.net = vm.Invoice.net;

                rec.payForId = vm.Invoice.payForId;
                rec.payFor = vm.Invoice.payFor;

                rec.isRefund = vm.Invoice.isRefund;

                // desktop كان بيرجعها false عند الحفظ
                rec.isReviewed = false;

                _db.SaveChanges();
                vm.Invoice.id = rec.id;
            }

            // ✅ مزامنة الحسابات
            UpdateDaily(vm.Invoice.id);

            return RedirectToAction("Index", new
            {
                showList = showList,
                Invoice_costcenterId = vm.Invoice.costcenterId,
                Invoice_date = vm.Invoice.date?.ToString("yyyy-MM-dd"),
                selectedId = vm.Invoice.id
            });
        }

        // ===============================
        // DELETE
        // ===============================
        public IActionResult Delete(
            int id,
            bool showList = true,
            int? Invoice_costcenterId = null,
            DateTime? Invoice_date = null
        )
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح لك بالحذف");

            var rec = _db.ConInvoices.FirstOrDefault(x => x.id == id);
            if (rec == null)
                return BadRequest("السجل غير موجود");

            // ❌ موقع غير مسموح
            if (!rec.costcenterId.HasValue || !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // ❌ ممنوع حذف بعد المراجعة
            var userName = HttpContext.Session.GetString("UserName");
            if (rec.isReviewed == true && userName != "fathi")
                return Forbid("لا يمكن الحذف بعد المراجعة (يسمح فقط للمدير)");

            // ✅ مسح قيود اليومية المرتبطة أولاً (لتجنب مشاكل الـ Foreign Key)
            var oldDailies = _db.acc_Daily.Where(i => i.invoiceId == id).ToList();
            if (oldDailies.Any())
            {
                _db.acc_Daily.RemoveRange(oldDailies);
            }

            _db.ConInvoices.Remove(rec);
            _db.SaveChanges();

            return RedirectToAction("Index", new
            {
                showList = showList,
                Invoice_costcenterId = Invoice_costcenterId ?? rec.costcenterId,
                Invoice_date = (Invoice_date ?? rec.date)?.ToString("yyyy-MM-dd")
            });
        }

        // ===============================
        // REVIEW (AJAX)
        // ===============================
        public class ReviewVM
        {
            public int id { get; set; }
            public bool reviewed { get; set; }
        }

        [HttpPost]
        public IActionResult Review([FromBody] ReviewVM model)
        {
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var rec = _db.ConInvoices.FirstOrDefault(x => x.id == model.id);
            if (rec == null)
                return BadRequest("السجل غير موجود");

            if (!rec.costcenterId.HasValue ||
                !PermissionHelper.CanCostCenter(rec.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            rec.isReviewed = model.reviewed;
            _db.SaveChanges();

            return Json(true);
        }


        // ===============================
        // Get Invoice Codes by CostCenter
        // ===============================
        [HttpGet]
        public IActionResult GetInvoiceCodes(int costCenterId)
        {
            if (!PermissionHelper.CanCostCenter(costCenterId, HttpContext))
                return Json(new List<string>());

            var codes = _db.ConInvoices.AsNoTracking()
                .Where(x => x.costcenterId == costCenterId && x.invoiceCode != null)
                .Select(x => x.invoiceCode!)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return Json(codes);
        }

        // ===============================
        // Summary like Desktop (deductions/remains)
        // ===============================
        [HttpGet]
        public IActionResult GetInvoiceSummary(int costCenterId, string invoiceCode)
        {
            if (!PermissionHelper.CanCostCenter(costCenterId, HttpContext))
                return Json(new { ok = false });

            if (string.IsNullOrWhiteSpace(invoiceCode))
                return Json(new { ok = false });

            var invs = _db.ConInvoices.AsNoTracking()
                .Where(x => x.costcenterId == costCenterId && x.invoiceCode == invoiceCode)
                .ToList();

            if (!invs.Any())
                return Json(new { ok = false });

            var balance = invs.First().balance ?? 0;
            var totalDeduction = invs.Sum(x => x.net ?? 0);
            var remains = balance - totalDeduction;

            return Json(new { ok = true, balance, totalDeduction, remains });
        }

        // ===============================
        // EXPORT EXCEL
        // ===============================
        public IActionResult ExportExcel(int? costCenterId = null)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Print", HttpContext))
                return Forbid("غير مسموح لك بالطباعة");

            var q = _db.ConInvoices.AsNoTracking().AsQueryable();

            if (costCenterId.HasValue)
            {
                if (!PermissionHelper.CanCostCenter(costCenterId.Value, HttpContext))
                    return Forbid("غير مسموح بالموقع");

                q = q.Where(x => x.costcenterId == costCenterId.Value);
            }

            var data = q.OrderByDescending(x => x.date).ThenBy(x => x.invoiceCode).ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("المستخلصات");

            ws.Cell(1, 1).Value = "رقم المستخلص";
            ws.Cell(1, 2).Value = "النوع";
            ws.Cell(1, 3).Value = "البيان";
            ws.Cell(1, 4).Value = "المعامل";
            ws.Cell(1, 5).Value = "قيمة المعامل";
            ws.Cell(1, 6).Value = "إجمالي الأعمال";
            ws.Cell(1, 7).Value = "الصافي";
            ws.Cell(1, 8).Value = "التاريخ";
            ws.Cell(1, 9).Value = "الموقع";
            ws.Cell(1, 10).Value = "مراجع";
            ws.Cell(1, 11).Value = "ترد";

            int row = 2;
            foreach (var i in data)
            {
                ws.Cell(row, 1).Value = i.invoiceCode;
                ws.Cell(row, 2).Value = i.invoiceType;
                ws.Cell(row, 3).Value = i.payFor;
                ws.Cell(row, 4).Value = i.factor;
                ws.Cell(row, 5).Value = i.factorValue;
                ws.Cell(row, 6).Value = i.balance;
                ws.Cell(row, 7).Value = i.net;
                ws.Cell(row, 8).Value = i.date?.ToString("yyyy-MM-dd");
                ws.Cell(row, 9).Value = i.costcenter;
                ws.Cell(row, 10).Value = (i.isReviewed == true ? "نعم" : "لا");
                ws.Cell(row, 11).Value = (i.isRefund == true ? "نعم" : "لا");
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"مستخلصات_{DateTime.Now:yyyy-MM-dd}.xlsx"
            );
        }

        // ===============================
        // UPDATE DAILY (SYNC ACCOUNTS)
        // ===============================
        private void UpdateDaily(int id)
        {
            // حذف القديم
            var oldDailies = _db.acc_Daily.Where(i => i.invoiceId == id).ToList();
            if (oldDailies.Any())
            {
                _db.acc_Daily.RemoveRange(oldDailies);
                _db.SaveChanges();
            }

            var rec = _db.ConInvoices.AsNoTracking().FirstOrDefault(x => x.id == id);
            if (rec == null) return;

            var userId = HttpContext.Session.GetInt32("UserId");

            // قيد جديد
            var daily = new acc_Daily
            {
                insertDate = DateTime.Now,
                insertUserId = userId,
                processDate = rec.date ?? DateTime.Now,
                costcenter = rec.costcenter,
                costcenterId = rec.costcenterId,
                dealer = rec.payFor,
                discount = 0,
                total = rec.net ?? 0,
                net = rec.net ?? 0,
                notes = $"مستخلص رقم #{rec.payFor}#باجمالي اعمال #{rec.net}#",
                invoiceCode = rec.invoiceCode,
                invoiceId = id
            };
            _db.acc_Daily.Add(daily);

            if (rec.isRefund == true)
            {
                var refundDaily = new acc_Daily
                {
                    insertDate = DateTime.Now,
                    insertUserId = userId,
                    processDate = rec.date ?? DateTime.Now,
                    costcenter = rec.costcenter,
                    costcenterId = rec.costcenterId,
                    dealer = rec.payFor,
                    discount = 0,
                    total = (rec.net ?? 0) * -1,
                    net = (rec.net ?? 0) * -1,
                    notes = $"رد قيمة مستخلص رقم #{rec.payFor}#باجمالي اعمال #{rec.net}#",
                    invoiceCode = rec.invoiceCode,
                    invoiceId = id
                };
                _db.acc_Daily.Add(refundDaily);
            }

            _db.SaveChanges();
        }
    }
}
