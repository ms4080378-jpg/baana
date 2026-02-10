using elbanna.Helpers; // ✅ PermissionHelper + Screens
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YourProject.Data;
using YourProject.Models;
using YourProject.ViewModels;

namespace YourProject.Controllers
{
    public class ItemPurchaseController : Controller
    {
        private readonly ItemPurchaseRepository _repo;

        // ✅ غيّرها لو اسم الشاشة مختلف عندك
        private const int SCREEN_ID = (int)Screens.ItemPurchase;

        public ItemPurchaseController(ItemPurchaseRepository repo)
        {
            _repo = repo;
        }

        // =========================
        // Helpers: Allowed CostCenters
        // =========================
        private List<SelectListItem> GetAllowedCostCentersSelectList()
        {
            var all = _repo.GetCostCenters().ToList();

            var allowed = all
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.costCenter
                })
                .ToList();

            return allowed;
        }

        private HashSet<int> GetAllowedCostCenterIds()
        {
            var all = _repo.GetCostCenters().ToList();
            return all
                .Where(cc => PermissionHelper.CanCostCenter(cc.id, HttpContext))
                .Select(cc => cc.id)
                .ToHashSet();
        }

        private void FillDropdowns(ItemPurchasePageVM vm)
        {
            // ✅ المواقع المسموح بها فقط
            vm.Filter.CostCenters = GetAllowedCostCentersSelectList();

            vm.Filter.Dealers = _repo.GetDealers()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.dealer
                }).ToList();

            vm.Filter.Items = _repo.GetItems()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.item
                }).ToList();

            vm.Form.CostCenters = vm.Filter.CostCenters;
            vm.Form.Dealers = vm.Filter.Dealers;
            vm.Form.Items = vm.Filter.Items;
        }

        // =====================================================
        // MAIN PAGE
        // =====================================================
        public IActionResult Index(ItemPurchaseFilterVM filter)
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var vm = new ItemPurchasePageVM { Filter = filter };
            FillDropdowns(vm);

            // ✅ تأمين: لو المستخدم اختار CostCenter مش مسموح → رجّعه فاضي
            if (filter.CostCenterId.HasValue && filter.CostCenterId.Value != 0)
            {
                if (!PermissionHelper.CanCostCenter(filter.CostCenterId.Value, HttpContext))
                {
                    vm.Grid = new List<ItemPurchaseGridRowVM>();
                    return View(vm);
                }
            }

            var allowedIds = GetAllowedCostCenterIds();

            var query = _repo.Query().AsNoTracking();

            // ✅ لو CostCenterId = 0 (الكل) → اعرض فقط المسموح
            query = query.Where(x =>
                x.costcenterId.HasValue &&
                allowedIds.Contains(x.costcenterId.Value)
            );

            #region Filters
            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.processDate >= filter.DateFrom);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.processDate <= filter.DateTo);

            if (filter.CostCenterId.HasValue && filter.CostCenterId.Value != 0)
                query = query.Where(x => x.costcenterId == filter.CostCenterId);

            if (filter.DealerId.HasValue)
                query = query.Where(x => x.dealerId == filter.DealerId);

            if (filter.ItemId.HasValue)
                query = query.Where(x => x.itemId == filter.ItemId);

            if (!string.IsNullOrWhiteSpace(filter.Building))
                query = query.Where(x => x.building == filter.Building);

            if (!string.IsNullOrWhiteSpace(filter.Floor))
                query = query.Where(x => x.floor == filter.Floor);

            if (!string.IsNullOrWhiteSpace(filter.Unit))
                query = query.Where(x => x.unit == filter.Unit);
            #endregion

            vm.Grid = query
                .Select(x => new ItemPurchaseGridRowVM
                {
                    Id = x.id,

                    Dealer = x.dealer ?? string.Empty,
                    Item = x.item ?? string.Empty,
                    CostCenter = x.costcenter ?? string.Empty,

                    ProcessDate = x.processDate,

                    Qty = x.qty ?? 0,
                    UnitPrice = x.unitPrice ?? 0,
                    Total = x.total ?? 0,

                    Building = x.building ?? string.Empty,
                    Floor = x.floor ?? string.Empty,
                    Unit = x.unit ?? string.Empty,

                    IsReviewed = x.isreviewed ?? false,
                    UserName = ""
                })
                .OrderByDescending(x => x.ProcessDate ?? DateTime.MinValue)
                .ToList();

            return View(vm);
        }

        // =====================================================
        // CREATE
        // =====================================================
        [HttpPost]
        public IActionResult Create(ItemPurchaseFormVM form)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                return Forbid("غير مسموح لك بالحفظ");

            if (!form.CostCenterId.HasValue || form.CostCenterId == 0)
                return BadRequest("يجب اختيار موقع صحيح");

            // ❌ موقع غير مسموح
            if (!PermissionHelper.CanCostCenter(form.CostCenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            if (string.IsNullOrWhiteSpace(form.CostCenter) || form.CostCenter == "الكل")
                return BadRequest("يجب اختيار موقع صحيح");

            if (!form.ItemId.HasValue || form.ItemId == 0)
                return BadRequest("الصنف مطلوب");

            if (form.Qty <= 0 || form.UnitPrice <= 0)
                return BadRequest("الكمية والسعر مطلوبين");

            var entity = new pr_itempurchase
            {
                processDate = form.ProcessDate,
                costcenterId = form.CostCenterId.Value,
                costcenter = form.CostCenter,
                dealerId = form.DealerId,
                dealer = form.Dealer,
                itemId = form.ItemId,
                item = form.Item,
                building = form.Building,
                floor = form.Floor,
                unit = form.Unit,
                qty = form.Qty,
                unitPrice = form.UnitPrice,
                total = form.Qty * form.UnitPrice,
                isreviewed = false,
                insertDate = DateTime.Now,
                insertUserId = (HttpContext.Session.GetInt32("UserId") ?? 0) == 0
                    ? (int?)null
                    : (HttpContext.Session.GetInt32("UserId") ?? 0)
            };

            _repo.Add(entity);

            return Ok();
        }

        // =====================================================
        // EDIT
        // =====================================================
        [HttpPost]
        public IActionResult Edit(ItemPurchaseFormVM form)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                return Forbid("غير مسموح لك بالتعديل");

            if (form.Id == 0)
                return BadRequest("رقم السجل غير صحيح");

            var entity = _repo.Query().FirstOrDefault(x => x.id == form.Id);
            if (entity == null)
                return NotFound("السجل غير موجود");

            // ❌ لو السجل موقعه مش مسموح
            if (!entity.costcenterId.HasValue || !PermissionHelper.CanCostCenter(entity.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // ⛔ لو تمت المراجعة امنع التعديل (نفس منطق BankTransfer/Daily)
            if (entity.isreviewed == true)
                return Forbid("لا يمكن التعديل بعد المراجعة");

            // كمان تأمين الـ CostCenter اللي جاي من الفورم
            if (!form.CostCenterId.HasValue || form.CostCenterId.Value == 0)
                return BadRequest("يجب اختيار موقع صحيح");

            if (!PermissionHelper.CanCostCenter(form.CostCenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            entity.processDate = form.ProcessDate;
            entity.costcenterId = form.CostCenterId.Value;
            entity.costcenter = form.CostCenter;

            entity.dealerId = form.DealerId;
            entity.dealer = form.Dealer;

            entity.itemId = form.ItemId;
            entity.item = form.Item;

            entity.building = form.Building;
            entity.floor = form.Floor;
            entity.unit = form.Unit;

            entity.qty = form.Qty;
            entity.unitPrice = form.UnitPrice;
            entity.total = form.Qty * form.UnitPrice;

            entity.lastUpdateDate = DateTime.Now;
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            entity.lastUpdateUserId = userId == 0 ? (int?)null : userId;
            entity.lastUpdateUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            _repo.Update(entity);

            return Ok();
        }

        // =====================================================
        // DELETE
        // =====================================================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح لك بالحذف");

            var entity = _repo.Query().FirstOrDefault(x => x.id == id);
            if (entity == null)
                return NotFound("السجل غير موجود");

            // ❌ لو السجل موقعه مش مسموح
            if (!entity.costcenterId.HasValue || !PermissionHelper.CanCostCenter(entity.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            // ⛔ لو تمت المراجعة امنع الحذف
            if (entity.isreviewed == true)
                return Forbid("لا يمكن الحذف بعد المراجعة");

            _repo.Delete(id);
            return Ok();
        }

        // =====================================================
        // GET LIST (AJAX)
        // =====================================================
        [HttpGet]
        public IActionResult GetList(ItemPurchaseFilterVM filter)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            var allowedIds = GetAllowedCostCenterIds();

            var query = _repo.Query().AsNoTracking();

            // ✅ دايمًا فلترة على المواقع المسموح بها
            query = query.Where(x =>
                x.costcenterId.HasValue &&
                allowedIds.Contains(x.costcenterId.Value)
            );

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.processDate >= filter.DateFrom);

            if (filter.DateTo.HasValue)
            {
                var toDate = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.processDate < toDate);
            }

            if (filter.CostCenterId.HasValue && filter.CostCenterId.Value != 0)
            {
                // ❌ لو طلب موقع مش مسموح
                if (!PermissionHelper.CanCostCenter(filter.CostCenterId.Value, HttpContext))
                    return Json(new List<object>());

                query = query.Where(x => x.costcenterId == filter.CostCenterId.Value);
            }

            var data = query
                .Select(x => new
                {
                    id = x.id,

                    userName = _repo.Users()
                        .Where(u => u.id == x.insertUserId)
                        .Select(u => u.username)
                        .FirstOrDefault() ?? "",

                    unit = x.unit ?? "",
                    floor = x.floor ?? "",
                    building = x.building ?? "",

                    item = x.item ?? "",
                    unitPrice = x.unitPrice ?? 0,

                    processDate = x.processDate,

                    costCenter = x.costcenter ?? "",
                    qty = x.qty ?? 0,
                    total = x.total ?? 0,

                    dealer = x.dealer ?? "",
                    isReviewed = x.isreviewed
                })
                .OrderByDescending(x => x.processDate ?? DateTime.MinValue)
                .ToList();

            return Json(data);
        }

        // =====================================================
        // TOGGLE REVIEW
        // =====================================================
        [HttpPost]
        public IActionResult ToggleReview(int id)
        {
            // ✅ مراجع أو فتحي فقط
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var row = _repo.Query().FirstOrDefault(x => x.id == id);
            if (row == null)
                return NotFound();

            if (!row.costcenterId.HasValue ||
                !PermissionHelper.CanCostCenter(row.costcenterId.Value, HttpContext))
                return Forbid("غير مسموح بالموقع");

            bool current = row.isreviewed ?? false;

            row.isreviewed = !current;
            row.lastUpdateDate = DateTime.Now;

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            row.lastUpdateUserId = userId == 0 ? (int?)null : userId;

            _repo.Update(row);

            return Ok(new { isReviewed = row.isreviewed });
        }


        // =====================================================
        // AJAX: GetInspects / CalcTotal (اختياري تأمين)
        // =====================================================
        [HttpGet]
        public IActionResult GetInspects(int costCenterId, int itemId)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            if (!PermissionHelper.CanCostCenter(costCenterId, HttpContext))
                return Forbid("غير مسموح بالموقع");

            var data = _repo.GetInspects(costCenterId, itemId)
                .Select(x => new
                {
                    x.building,
                    x.floor,
                    x.unit,
                    x.qty,
                    x.notes
                });

            return Json(data);
        }

        [HttpGet]
        public IActionResult CalcTotal(decimal qty, decimal price)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            return Json(qty * price);
        }
    }
}


