using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class OutTrnsController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.OutTrns;

        public OutTrnsController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // Helper: Allowed CostCenters
        // =========================
        private int[] GetAllowedCostCentersForUser()
        {
            // نفس فكرة BankTransfer: فلترة كل المواقع حسب PermissionHelper
            return _context.acc_CostCenters
                .AsNoTracking()
                .Select(x => x.id)
                .ToList()
                .Where(id => PermissionHelper.CanCostCenter(id, HttpContext))
                .ToArray();
        }

        // =========================
        // Index
        // =========================
        public IActionResult Index()
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            // ✅ صلاحيات الأزرار
            ViewBag.CanAdd = PermissionHelper.Can(SCREEN_ID, "Add", HttpContext);
            ViewBag.CanEdit = PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext);
            ViewBag.CanDelete = PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext);
            ViewBag.CanList = true;

            // ✅ مواقع مسموحة للـ View
            ViewBag.AllowedCostCenters = GetAllowedCostCentersForUser();

            return View();
        }

        // =========================
        // Save (Add / Edit)
        // =========================
        [HttpPost]
        public IActionResult Save([FromBody] OutTrnsVM model)
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid("غير مسموح");

            if (model == null)
                return BadRequest("بيانات غير صحيحة");

            if (model.CostCenterId <= 0)
                return BadRequest("يجب اختيار الموقع");

            // ❌ موقع غير مسموح
            if (!PermissionHelper.CanCostCenter(model.CostCenterId, HttpContext))
                return Forbid("غير مسموح بالموقع");

            if (model.ItemId <= 0)
                return BadRequest("يجب اختيار الصنف");

            if (model.Qty <= 0)
                return BadRequest("الكمية يجب أن تكون أكبر من صفر");

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

            pr_outtrns row;

            // ✅ userId من السيشن
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // =========================
            // Add / Edit
            // =========================
            if (model.Id == 0)
            {
                row = new pr_outtrns();
                row.insertDate = DateTime.Now;
                row.insertUserId = userId == 0 ? null : userId;
                row.isreviewed = false;
                _context.pr_outtrns.Add(row);
            }
            else
            {
                row = _context.pr_outtrns.FirstOrDefault(x => x.id == model.Id);
                if (row == null)
                    return BadRequest("السجل غير موجود");

                // ❌ تأمين موقع السجل الحالي (مهم جدًا)
                if (!row.costcenterid.HasValue || !PermissionHelper.CanCostCenter(row.costcenterid.Value, HttpContext))
                    return Forbid("غير مسموح بالموقع");

                // ⛔ لو تمت المراجعة امنع التعديل
                if (row.isreviewed == true)
                    return Forbid("لا يمكن التعديل بعد المراجعة");

                row.lastUpdateDate = DateTime.Now;
                row.lastUpdateUserId = userId == 0 ? null : userId;
            }

            // =========================
            // Date
            // =========================
            row.processDate = model.ProcessDate.Date;

            // =========================
            // Cost Center (✅ توحيد DbSet)
            // =========================
            var cost = _context.acc_CostCenters
                .Where(x => x.id == model.CostCenterId)
                .Select(x => new { x.id, x.costCenter })
                .FirstOrDefault();

            if (cost == null)
                return BadRequest("الموقع غير موجود");

            row.costcenterid = cost.id;
            row.costcenter = cost.costCenter;

            // =========================
            // Item (FK → IC_ItemStore)
            // =========================
            var item = _context.IC_ItemStore
                .Where(x => x.id == model.ItemId)
                .Select(x => new { x.id, x.item })
                .FirstOrDefault();

            if (item == null)
                return BadRequest("الصنف غير موجود في المخزن");

            row.itemid = item.id;
            row.item = item.item;

            // =========================
            // Other
            // =========================
            row.qty = model.Qty;
            row.notes = model.Notes;

            _context.SaveChanges();

            return Ok(model.Id == 0 ? "تم الحفظ بنجاح" : "تم التعديل بنجاح");
        }

        // =========================
        // List
        // =========================
        [HttpGet]
        public IActionResult List(int costCenterId = 0, int itemId = 0, DateTime? from = null, DateTime? to = null)
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Json(new object[] { });

            var query = _context.pr_outtrns.AsNoTracking();

            // ✅ فلترة إجبارية بالمواقع المسموحة
            var allowed = GetAllowedCostCentersForUser();
            if (allowed != null && allowed.Length > 0)
                query = query.Where(x => x.costcenterid.HasValue && allowed.Contains(x.costcenterid.Value));

            // ✅ لو محدد موقع بعينه لازم يكون مسموح
            if (costCenterId > 0)
            {
                if (allowed != null && allowed.Length > 0 && !allowed.Contains(costCenterId))
                    return Json(new object[] { });

                query = query.Where(x => x.costcenterid == costCenterId);
            }

            if (itemId > 0)
                query = query.Where(x => x.itemid == itemId);

            if (from.HasValue)
                query = query.Where(x => x.processDate.HasValue &&
                                         x.processDate.Value >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.processDate.HasValue &&
                                         x.processDate.Value <= to.Value);

            var data = query
                .OrderByDescending(x => x.id)
                .Take(200)
                .Select(x => new
                {
                    x.id,
                    userName = _context.hr_user
                        .Where(u => u.id == x.insertUserId)
                        .Select(u => u.username)
                        .FirstOrDefault() ?? "",
                    costCenterId = x.costcenterid,
                    itemId = x.itemid,
                    costCenter = x.costcenter ?? "",
                    item = x.item ?? "",
                    processDate = x.processDate,
                    qty = x.qty ?? 0,
                    notes = x.notes ?? ""
                })
                .ToList();

            return Json(data);
        }

        // =========================
        // Cost Centers
        // =========================
        [HttpGet]
        public IActionResult GetCostCenters()
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Json(new object[] { });

            // ✅ رجّع المسموح فقط
            var allowed = GetAllowedCostCentersForUser();

            var data = _context.acc_CostCenters
                .AsNoTracking()
                .Where(x => allowed.Contains(x.id))
                .OrderBy(x => x.costCenter)
                .Select(x => new
                {
                    id = x.id,
                    name = x.costCenter
                })
                .ToList();

            return Json(data);
        }

        // =========================
        // Items
        // =========================
        [HttpGet]
        public IActionResult GetItems()
        {
            // (اختياري) امنع لو مش مسموح يفتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Json(new object[] { });

            var data = _context.IC_ItemStore
                .AsNoTracking()
                .Where(x => x.item != null)
                .OrderBy(x => x.item)
                .Select(x => new
                {
                    id = x.id,
                    name = x.item
                })
                .ToList();

            return Json(data);
        }

        // =========================
        // Delete
        // =========================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid("غير مسموح");

            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح لك بالحذف");

            var row = _context.pr_outtrns.FirstOrDefault(x => x.id == id);
            if (row == null)
                return BadRequest("السجل غير موجود");

            // ❌ موقع غير مسموح
            if (!row.costcenterid.HasValue || !PermissionHelper.CanCostCenter(row.costcenterid.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            _context.pr_outtrns.Remove(row);
            _context.SaveChanges();

            return Ok("تم الحذف");
        }
    }
}
