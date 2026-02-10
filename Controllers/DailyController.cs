using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class DailyController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.Daily;

        public DailyController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // Index
        // =========================
        public IActionResult Index()
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var allCC = _context.acc_CostCenter
    .AsNoTracking()
    .ToList();

            var allowedCC = allCC
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .ToList();

            var vm = new DailyIndexVM
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                Date = DateTime.Today,
                CostCenters = allowedCC,
                CreditAccounts = _context.acc_CreditAccounts.ToList()
            };


            vm.Dealers = _context.Dealers
                .AsNoTracking()
                .ToList();

            return View(vm);
        }

        // =========================
        // Save
        // =========================
        [HttpPost]
        public IActionResult Save([FromBody] DailyIndexVM model)
        {
            if (model == null)
                return BadRequest("Model null");

            if (model.CostCenterId <= 0)
                return BadRequest("من فضلك اختر الموقع");

            // ❌ صلاحيات إضافة / تعديل
            if (model.Id == 0 &&
                !PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                return Forbid();

            if (model.Id > 0 &&
                !PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                return Forbid();

            // ❌ موقع غير مسموح
            if (!PermissionHelper.CanCostCenter(model.CostCenterId, HttpContext))
                return Forbid();

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            acc_Daily daily;

            if (model.Id > 0)
            {
                daily = _context.acc_Daily.Find(model.Id);
                if (daily == null)
                    return NotFound();

                // ❌ ممنوع تعديل سجل متراجع (زي الديسك توب)
                if (daily.isReviewed == true)
                    return Forbid("لا يمكن التعديل بعد المراجعة");

                daily.lastUpdateDate = DateTime.Now;
                daily.lastUpdateUserId = userId;
            }

            else
            {
                daily = new acc_Daily
                {
                    insertDate = DateTime.Now,
                    insertUserId = userId
                };
                _context.acc_Daily.Add(daily);
            }

            // ===============================
            // الإيصال
            int? invoiceId = null;
            if (!string.IsNullOrWhiteSpace(model.Receipt))
            {
                invoiceId = _context.acc_invoiceCode
                    .Where(x => x.invoiceCode == model.Receipt)
                    .Select(x => (int?)x.id)
                    .FirstOrDefault();
            }

            // ===============================
            // Assign
            daily.processDate = model.Date;
            daily.dealerCode = model.DealerCode;
            daily.dealer = model.DealerName;

            daily.total = model.Total;
            daily.discount = model.Discount;
            daily.net = model.Net;

            daily.fromAcc = model.FromAccName;
            daily.toAcc = model.ToAccName;

            daily.costcenterId = model.CostCenterId;
            daily.costcenter = model.CostCenterName;

            daily.invoiceId = invoiceId;
            daily.invoiceCode = model.Receipt?.Trim();
            daily.notes = model.Notes;

            _context.SaveChanges();

            return Ok(model.Id > 0 ? "تم التعديل بنجاح" : "تم الحفظ بنجاح");
        }

        // =========================
        // List
        // =========================
        [HttpGet]
        public IActionResult List(DateTime from, DateTime to, int? costCenterId)
        {
            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1);

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var allowOther = HttpContext.Session.GetInt32("AllowShowOtherData") == 1;

            // =========================
            // 1️⃣ تحميل من DB فقط
            // =========================
            var rows = _context.acc_Daily
                .AsNoTracking()
                .Where(x =>
                    x.processDate >= fromDate &&
                    x.processDate < toDate &&
                    (!costCenterId.HasValue || x.costcenterId == costCenterId)
                )
                .ToList();

            // =========================
            // 2️⃣ فلترة الصلاحيات (In-Memory)
            // =========================
            rows = rows
                .Where(x =>
                    x.costcenterId.HasValue &&
                    PermissionHelper.CanCostCenter(x.costcenterId.Value, HttpContext) &&
                    (
                        allowOther ||
                        x.insertUserId == userId ||
                        x.lastUpdateUserId == userId
                    )
                )
                .ToList();

            // =========================
            // 3️⃣ Select النهائي
            // =========================
            var data = rows
                .OrderByDescending(x => x.processDate)
                .Select(x => new
                {
                    id = x.id,
                    date = x.processDate!.Value.ToString("yyyy-MM-dd"),
                    dateRaw = x.processDate,

                    costCenterId = x.costcenterId,
                    costCenter = x.costcenter,

                    dealer = x.dealer,
                    dealerCode = x.dealerCode,

                    total = x.total ?? 0,
                    discount = x.discount ?? 0,
                    net = x.net ?? 0,

                    fromAcc = x.fromAcc,
                    toAcc = x.toAcc,

                    receipt = x.invoiceCode,
                    notes = x.notes,

                    done = x.isReviewed ?? false,
                    spend = x.isAllowed ?? false,

                    // ✅ اسم المستخدم اللي حفظ + المستخدم اللي عدّل
                    userName = _context.hr_user
                        .Where(u => u.id == x.insertUserId)
                        .Select(u => u.username)
                        .FirstOrDefault() ?? "",
                    editUser = _context.hr_user
                        .Where(u => u.id == x.lastUpdateUserId)
                        .Select(u => u.username)
                        .FirstOrDefault() ?? ""
                })
                .Take(500)
                .ToList();

            return Json(data);
        }

        [HttpGet]
        public IActionResult GetDealerInfo(int id)
        {
            var dealer = _context.Dealers
                .AsNoTracking()
                .Where(x => x.id == id)
                .Select(x => new
                {
                    code = x.code,
                    nationalId = x.nationalId
                })
                .FirstOrDefault();

            if (dealer == null)
                return NotFound();

            return Json(dealer);
        }

        [HttpGet]
        public IActionResult Print(DateTime from, DateTime to, int? costCenterId)
        {
            // ❌ صلاحية الشاشة
            if (!PermissionHelper.Can(SCREEN_ID, "Print", HttpContext))
                return Forbid();

            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1);

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var allowOther = HttpContext.Session.GetInt32("AllowShowOtherData") == 1;

            // 1️⃣ بيانات من الداتابيز فقط
            var rows = _context.acc_Daily
                .AsNoTracking()
                .Where(x =>
                    x.processDate >= fromDate &&
                    x.processDate < toDate &&
                    (!costCenterId.HasValue || x.costcenterId == costCenterId)
                )
                .ToList();

            // 2️⃣ فلترة صلاحيات المواقع + المستخدم
            rows = rows
                .Where(x =>
                    x.costcenterId.HasValue &&
                    PermissionHelper.CanCostCenter(x.costcenterId.Value, HttpContext) &&
                    (
                        allowOther ||
                        x.insertUserId == userId ||
                        x.lastUpdateUserId == userId
                    )
                )
                .ToList();

            return View("Print", rows);
        }


        // =========================
        // Get By Id
        // =========================
        public IActionResult GetById(int id)
        {
            var d = _context.acc_Daily
                .Where(x => x.id == id)
                .Select(x => new
                {
                    id = x.id,
                    processDate = x.processDate,
                    dealerCode = x.dealerCode,
                    statement = x.dealer,
                    total = x.total,
                    discount = x.discount,
                    net = x.net,
                    fromAcc = x.fromAcc,
                    toAcc = x.toAcc,
                    receipt = x.invoiceCode,
                    notes = x.notes
                })
                .FirstOrDefault();

            return Json(d);
        }


        // =========================
        // Review
        // =========================
        [HttpPost]
        public IActionResult Review(int id)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                return Forbid();

            var item = _context.acc_Daily.Find(id);
            if (item == null)
                return NotFound();

            item.isReviewed = true;
            _context.SaveChanges();
            return Ok();
        }

        // =========================
        // Delete
        // =========================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid();

            var row = _context.acc_Daily.Find(id);
            if (row == null)
                return NotFound();

            if (!row.costcenterId.HasValue ||
    !PermissionHelper.CanCostCenter(row.costcenterId.Value, HttpContext))
                return Forbid();


            _context.acc_Daily.Remove(row);
            _context.SaveChanges();

            return Ok();
        }

        // =========================
        // Allow / Spend
        // =========================
        [HttpPost]
        public IActionResult SetAllowed([FromBody] ToggleVM model)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                return Forbid();

            var daily = _context.acc_Daily.Find(model.id);
            if (daily == null)
                return NotFound();

            daily.isAllowed = model.value;
            _context.SaveChanges();

            return Ok(new { success = true });
        }

        // =========================
        // Bulk Review
        // =========================
        public class BulkReviewDto
        {
            public List<int> ids { get; set; }
            public bool value { get; set; }
        }

        [HttpPost]
        public IActionResult SetReviewedBulk([FromBody] BulkReviewDto model)
        {
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");
            // 1️⃣ تحميل من DB فقط
            var rows = _context.acc_Daily
                .Where(x => model.ids.Contains(x.id))
                .ToList();

            // 2️⃣ فلترة الصلاحيات In-Memory
            rows = rows
                .Where(x =>
                    x.costcenterId.HasValue &&
                    PermissionHelper.CanCostCenter(x.costcenterId.Value, HttpContext)
                )
                .ToList();

            foreach (var row in rows)
                row.isReviewed = model.value;

            _context.SaveChanges();
            return Ok();
        }

        


        // =========================
        // Bulk Spend
        // =========================
        public class BulkSpendDto
        {
            public List<int> ids { get; set; }
            public bool value { get; set; }
        }

        [HttpPost]
        public IActionResult SetSpendBulk([FromBody] BulkSpendDto model)
        {
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");
            // 1️⃣ تحميل من DB
            var rows = _context.acc_Daily
                .Where(x => model.ids.Contains(x.id))
                .ToList();

            // 2️⃣ فلترة الصلاحيات
            rows = rows
                .Where(x =>
                    x.costcenterId.HasValue &&
                    PermissionHelper.CanCostCenter(x.costcenterId.Value, HttpContext)
                )
                .ToList();

            foreach (var row in rows)
                row.isAllowed = model.value;

            _context.SaveChanges();
            return Ok();
        }



    }
}
