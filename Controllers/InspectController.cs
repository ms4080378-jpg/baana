using elbanna.Data;
using elbanna.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YourProject.Models;
using YourProject.Models.ViewModels;

namespace YourProject.Controllers
{
    public class InspectController : Controller
    {
        private readonly AppDbContext _context;
        private const string SCREEN_NAME = "مقايسة الموقع";

        public InspectController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.CanAdd = PermissionHelper.Can(screenId, "Add", HttpContext);
            ViewBag.CanEdit = PermissionHelper.Can(screenId, "Edit", HttpContext);
            ViewBag.CanDelete = PermissionHelper.Can(screenId, "Delete", HttpContext);
            ViewBag.CanPrint = PermissionHelper.Can(screenId, "Print", HttpContext);

            return View(LoadVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save([FromBody] InspectVM model)
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");

            // Add/Edit
            if (model != null && model.Id == 0)
            {
                if (!PermissionHelper.Can(screenId, "Add", HttpContext))
                    return Forbid("غير مسموح لك بالحفظ");
            }
            else
            {
                if (!PermissionHelper.Can(screenId, "Edit", HttpContext) && !PermissionHelper.Can(screenId, "Add", HttpContext))
                    return Forbid("غير مسموح لك بالتعديل");
            }

            // ===== امسح خصائص العرض =====
            ModelState.Remove(nameof(InspectVM.List));
            ModelState.Remove(nameof(InspectVM.InspectList));
            ModelState.Remove(nameof(InspectVM.ItemNames));
            ModelState.Remove(nameof(InspectVM.Items));
            ModelState.Remove(nameof(InspectVM.CostCenters));

            if (!ModelState.IsValid)
                return BadRequest("بيانات غير صحيحة");

            bool exists = _context.pr_Inspect.Any(x =>
                x.itemId == model.ItemId &&
                x.costcenterId == model.CostCenterId &&
                x.building == model.Building &&
                x.floor == model.Floor &&
                x.unit == model.Unit &&
                x.id != model.Id
            );

            if (exists)
                return Conflict("هذا الصنف موجود مسبقًا");

            pr_Inspect inspect;

            if (model.Id == 0)
            {
                inspect = new pr_Inspect();
                _context.pr_Inspect.Add(inspect);
            }
            else
            {
                inspect = _context.pr_Inspect.Find(model.Id);
                inspect.lastUpdateDate = DateTime.Now;
                inspect.lastUpdateUserId = 1;
            }

            // ===== حفظ البيانات الأساسية =====
            inspect.itemId = model.ItemId;
            inspect.costcenterId = model.CostCenterId;

            // ✅ السطرين المهمين جدًا (حفظ اسم الموقع)
            inspect.costcenter = _context.acc_CostCenter
                .Where(x => x.id == model.CostCenterId)
                .Select(x => x.costCenter)
                .FirstOrDefault();

            inspect.building = model.Building;
            inspect.floor = model.Floor;
            inspect.unit = model.Unit;
            inspect.qty = model.Qty;

            _context.SaveChanges();

            return Ok(new { message = "تم الحفظ بنجاح" });
        }




        private InspectVM LoadVM(InspectVM vm = null)
        {
            vm ??= new InspectVM();

            vm.Items = _context.IC_ItemStore
                .Where(x => x.isVendorItem == true)
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.item
                }).ToList();

            vm.CostCenters = _context.acc_CostCenter
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.costCenter
                }).ToList();

            if (vm.CostCenterId != null)
            {
                vm.List = _context.pr_Inspect
                    .Where(x => x.costcenterId == vm.CostCenterId)
                    .ToList();

                var itemIds = vm.List
                    .Where(x => x.itemId != null)
                    .Select(x => x.itemId.Value)
                    .Distinct()
                    .ToList();

                vm.ItemNames = _context.IC_ItemStore
                    .Where(x => itemIds.Contains(x.id))
                    .ToDictionary(x => x.id, x => x.item);
            }


            return vm;
        }


        [HttpGet]
        public IActionResult GetList(int costCenterId)
        {
            var screenId = ScreenRegistry.EnsureScreen(_context, SCREEN_NAME);
            if (!PermissionHelper.CanOpenScreen(screenId, HttpContext))
                return Forbid("غير مسموح");

            var list =
                (from i in _context.pr_Inspect
                 join item in _context.IC_ItemStore
                     on i.itemId equals item.id into itemJoin
                 from item in itemJoin.DefaultIfEmpty()
                 where i.costcenterId == costCenterId
                 select new
                 {
                     id = i.id,
                     item = item != null ? item.item : "",
                     costCenter = i.costcenter ?? "",
                     building = i.building ?? "",
                     floor = i.floor ?? "",
                     unit = i.unit ?? "",
                     qty = i.qty
                 }).ToList();

            return Json(list);
        }





    }
}
