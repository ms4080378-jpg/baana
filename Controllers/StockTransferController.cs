using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class StockTransferController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.StockTransfer;

        public StockTransferController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // Index
        // =========================
        public IActionResult Index()
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var allCC = _context.acc_CostCenter.AsNoTracking().ToList();
            var allowedCC = allCC
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .ToList();

            var vm = new StockTransferIndexVM
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                Date = DateTime.Today,

                CostCenters = allowedCC,

                Items = _context.IC_ItemStore
                    .Where(x => x.isStopped != true)
                    .Select(x => new Item
                    {
                        Id = x.id,
                        Name = x.item
                    })
                    .AsNoTracking()
                    .ToList()
            };

            return View(vm);
        }

        // =========================
        // Save
        // =========================
        [HttpPost]
        public IActionResult Save([FromBody] StockTransferIndexVM model)
        {
            if (model == null)
                return BadRequest("Model null");

            if (!model.FromCostCenterId.HasValue || !model.ToCostCenterId.HasValue)
                return BadRequest("الموقع مطلوب");

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

            // ✅ صلاحيات المواقع (From + To)
            if (!PermissionHelper.CanCostCenter(model.FromCostCenterId.Value, HttpContext) ||
                !PermissionHelper.CanCostCenter(model.ToCostCenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            if (!model.ItemId.HasValue || model.ItemId.Value <= 0)
                return BadRequest("الصنف مطلوب");

            if (model.Qty <= 0)
                return BadRequest("الكمية غير صحيحة");

            // ✅ تأمين UserId عشان FK hr_user
            var sessionUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            int? safeUserId = null;
            if (sessionUserId > 0 && _context.hr_user.Any(x => x.id == sessionUserId))
                safeUserId = sessionUserId;

            IC_StockTransfer row;

            // =========================
            // Update
            // =========================
            if (model.Id > 0)
            {
                row = _context.IC_StockTransfers.Find(model.Id);
                if (row == null)
                    return NotFound();

                // ✅ تأمين إن السجل نفسه من مواقع مسموح بيها (From + To)
                if (row.costcenterId <= 0 ||
                    row.costcenterToId <= 0 ||
                    !PermissionHelper.CanCostCenter(row.costcenterId, HttpContext) ||
                    !PermissionHelper.CanCostCenter(row.costcenterToId, HttpContext))
                {
                    return Forbid("غير مسموح بالموقع");
                }

                row.lastUpdateDate = DateTime.Now;
                row.lastUpdateUserId = safeUserId;
            }
            // =========================
            // Add
            // =========================
            else
            {
                row = new IC_StockTransfer
                {
                    insertDate = DateTime.Now,
                    insertUserId = safeUserId,
                    lastUpdateDate = null,
                    lastUpdateUserId = null
                };

                _context.IC_StockTransfers.Add(row);
            }

            // =========================
            // Date
            // =========================
            row.processDate = model.Date.Date;

            // =========================
            // From Cost Center (name)
            // =========================
            var fromCC = _context.acc_CostCenter
                .Where(x => x.id == model.FromCostCenterId.Value)
                .Select(x => x.costCenter)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(fromCC))
                return BadRequest("موقع الصرف غير موجود");

            row.costcenterId = model.FromCostCenterId.Value;
            row.costcenter = fromCC;

            // =========================
            // To Cost Center (name)
            // =========================
            var toCC = _context.acc_CostCenter
                .Where(x => x.id == model.ToCostCenterId.Value)
                .Select(x => x.costCenter)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(toCC))
                return BadRequest("موقع الاستلام غير موجود");

            row.costcenterToId = model.ToCostCenterId.Value;
            row.costcenterTo = toCC;

            // =========================
            // Item (FK safe)
            // =========================
            var itemStore = _context.IC_ItemStore
                .Where(x => x.id == model.ItemId.Value && x.isStopped != true)
                .Select(x => new { x.id, x.item })
                .FirstOrDefault();

            if (itemStore == null)
                return BadRequest("الصنف غير موجود بالمخزن");

            row.itemid = itemStore.id;
            row.item = itemStore.item;

            // =========================
            // Qty
            // =========================
            row.qty = model.Qty;

            _context.SaveChanges();

            return Ok(model.Id > 0 ? "تم التعديل بنجاح" : "تم الحفظ بنجاح");
        }

        // =========================
        // List
        // =========================
        [HttpGet]
        public IActionResult List(DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1);

            // 1) تحميل من DB فقط
            var rows = _context.IC_StockTransfers
                .AsNoTracking()
                .Where(x => x.processDate >= fromDate && x.processDate < toDate)
                .ToList();

            // 2) فلترة صلاحيات المواقع (From + To)
            rows = rows
                .Where(x =>
                    x.costcenterId > 0 &&
                    x.costcenterToId > 0 &&
                    PermissionHelper.CanCostCenter(x.costcenterId, HttpContext) &&
                    PermissionHelper.CanCostCenter(x.costcenterToId, HttpContext)
                )
                .ToList();

            var data = rows
                .OrderByDescending(x => x.processDate)
                .Select(x => new
                {
                    id = x.id,
                    date = x.processDate.HasValue ? x.processDate.Value.ToString("yyyy-MM-dd") : "",
                    fromCC = x.costcenter,
                    toCC = x.costcenterTo,
                    item = x.item,
                    qty = x.qty
                })
                .Take(500)
                .ToList();

            return Json(data);
        }

        // =========================
        // Get By Id
        // =========================
        public IActionResult GetById(int id)
        {
            var rowEntity = _context.IC_StockTransfers
                .AsNoTracking()
                .FirstOrDefault(x => x.id == id);

            if (rowEntity == null)
                return Json(null);

            // ✅ صلاحيات المواقع (From + To)
            if (rowEntity.costcenterId <= 0 ||
                rowEntity.costcenterToId <= 0 ||
                !PermissionHelper.CanCostCenter(rowEntity.costcenterId, HttpContext) ||
                !PermissionHelper.CanCostCenter(rowEntity.costcenterToId, HttpContext))
            {
                return Forbid("غير مسموح بالموقع");
            }

            var row = new
            {
                rowEntity.id,
                rowEntity.processDate,
                fromCostCenterId = rowEntity.costcenterId,
                toCostCenterId = rowEntity.costcenterToId,
                itemId = rowEntity.itemid,
                rowEntity.qty
            };

            return Json(row);
        }

        // =========================
        // Delete
        // =========================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح لك بالحذف");

            var row = _context.IC_StockTransfers.Find(id);
            if (row == null)
                return NotFound();

            // ✅ صلاحيات المواقع (From + To)
            if (row.costcenterId <= 0 ||
                row.costcenterToId <= 0 ||
                !PermissionHelper.CanCostCenter(row.costcenterId, HttpContext) ||
                !PermissionHelper.CanCostCenter(row.costcenterToId, HttpContext))
            {
                return Forbid("غير مسموح بالموقع");
            }

            _context.IC_StockTransfers.Remove(row);
            _context.SaveChanges();

            return Ok();
        }
    }
}
