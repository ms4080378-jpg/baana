using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class InTrnsController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.intrns; // ✅ غيّر لو اسمها مختلف في enum

        public InTrnsController(AppDbContext context)
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

            // ✅ صلاحيات الأزرار (لـ JS)
            ViewBag.CanAdd = PermissionHelper.Can(SCREEN_ID, "Add", HttpContext);
            ViewBag.CanEdit = PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext);
            ViewBag.CanDelete = PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext);
            ViewBag.CanList = true;

            // ✅ فلترة المواقع المسموح بها
            var allowedCC = _context.acc_CostCenters
                .AsNoTracking()
                .OrderBy(x => x.costCenter)
                .ToList()
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .ToList();

            ViewBag.AllowedCostCenters = allowedCC.Select(x => x.id).ToArray();

            var vm = new InTrnsPageVM
            {
                Filter = new InTrnsFilterVM
                {
                    CostCenters = allowedCC
                        .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = x.id.ToString(),
                            Text = x.costCenter
                        }).ToList()
                }
            };

            return View(vm);
        }

        // =========================
        // GET LIST
        // =========================
        [HttpGet]
        public IActionResult GetList(DateTime from, DateTime to, int costCenterId)
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Json(new object[] { });

            // ✅ allowed IDs
            var allowedIds = _context.acc_CostCenters
                .AsNoTracking()
                .Select(x => x.id)
                .ToList()
                .Where(id => PermissionHelper.CanCostCenter(id, HttpContext))
                .ToList();

            // ❌ لو محدد موقع لازم يكون مسموح
            if (costCenterId != 0 && !allowedIds.Contains(costCenterId))
                return Json(new object[] { });

            var toDate = to.Date.AddDays(1);

            var query = _context.pr_intrns.AsNoTracking()
                .Where(x =>
                    x.processDate >= from &&
                    x.processDate < toDate
                );

            // ✅ فلترة إجبارية للمسموح
            query = query.Where(x => x.costcenterId.HasValue && allowedIds.Contains(x.costcenterId.Value));

            // ✅ لو اختار موقع بعينه
            if (costCenterId != 0)
                query = query.Where(x => x.costcenterId == costCenterId);

            var data = query
                .Select(x => new
                {
                    id = x.id,
                    item = x.item ?? "",
                    costCenterId = x.costcenterId,
                    costCenter = x.costcenter ?? "",
                    processDate = x.processDate,
                    qty = x.qty ?? 0,
                    unitPrice = x.unitPrice ?? 0,
                    total = x.total ?? 0
                })
                .OrderByDescending(x => x.processDate)
                .ToList();

            return Json(data);
        }

        // =========================
        // SAVE (ADD / EDIT)
        // =========================
        [HttpPost]
        public IActionResult Save(InTrnsFormVM form)
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid("غير مسموح");

            if (form.CostCenterId == 0 || string.IsNullOrEmpty(form.Item) || form.Qty <= 0)
                return BadRequest("بيانات غير مكتملة");

            // ❌ موقع غير مسموح
            if (!PermissionHelper.CanCostCenter(form.CostCenterId, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // ✅ صلاحيات Add/Edit
            if (form.Id == 0)
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                    return Forbid("غير مسموح لك بالحفظ");
            }
            else
            {
                if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                    return Forbid("غير مسموح لك بالتعديل");
            }

            pr_intrn entity;

            if (form.Id == 0)
            {
                entity = new pr_intrn
                {
                    insertDate = DateTime.Now
                };
                _context.pr_intrns.Add(entity);
            }
            else
            {
                entity = _context.pr_intrns.FirstOrDefault(x => x.id == form.Id);
                if (entity == null)
                    return NotFound("السجل غير موجود");

                // ❌ تأمين موقع السجل الحالي
                if (!entity.costcenterId.HasValue || !PermissionHelper.CanCostCenter(entity.costcenterId.Value, HttpContext))
                    return Forbid("غير مسموح بالموقع");

                entity.lastUpdateDate = DateTime.Now;
            }

            entity.processDate = form.ProcessDate;
            entity.costcenterId = form.CostCenterId;
            entity.costcenter = form.CostCenter;

            // حفظ الصنف بالاسم فقط
            entity.item = form.Item;

            entity.qty = form.Qty;
            entity.unitPrice = form.UnitPrice;
            entity.total = form.Qty * form.UnitPrice;

            _context.SaveChanges();
            return Ok();
        }

        // =========================
        // ITEMS
        // =========================
        [HttpGet]
        public IActionResult GetItems()
        {
            // (اختياري) منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Json(new object[] { });

            var items = _context.IC_ItemStore
                .AsNoTracking()
                .Where(x => x.isStopped == null || x.isStopped == false)
                .Select(x => x.item)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return Json(items);
        }

        // =========================
        // DELETE
        // =========================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid("غير مسموح");

            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح لك بالحذف");

            var row = _context.pr_intrns.FirstOrDefault(x => x.id == id);
            if (row == null) return NotFound("السجل غير موجود");

            // ❌ موقع غير مسموح
            if (!row.costcenterId.HasValue || !PermissionHelper.CanCostCenter(row.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            _context.pr_intrns.Remove(row);
            _context.SaveChanges();
            return Ok();
        }
    }
}
