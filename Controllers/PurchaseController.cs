using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PurchaseController : Controller
{
    private readonly AppDbContext _context;
    private const int SCREEN_ID = (int)Screens.Purchase; // ✅ مهم جدًا

    public PurchaseController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // ❌ منع فتح الشاشة بدون صلاحية
        if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
            return RedirectToAction("AccessDenied", "Auth");

        // 1️⃣ تحميل كل المواقع من DB
        var allCostCenters = _context.acc_CostCenters
            .AsNoTracking()
            .ToList();

        // 2️⃣ فلترة الصلاحيات In-Memory
        var allowedCostCenters = allCostCenters
            .Where(c => PermissionHelper.CanCostCenter(c.id, HttpContext))
            .ToList();

        var vm = new PurchaseVM
        {
            Date = DateTime.Today,
            DateFrom = DateTime.Today,
            DateTo = DateTime.Today,

            CostCenters = allowedCostCenters,
            Dealers = _context.Dealers.AsNoTracking().ToList(),
            Items = _context.Items.AsNoTracking().ToList()
        };

        return View(vm);
    }









    public IActionResult GetList(DateTime fromDate, DateTime toDate, int costcenterId)
    {
        var rows = _context.Purchases
            .AsNoTracking()
            .Where(p =>
                p.processDate >= fromDate &&
                p.processDate <= toDate &&
                (costcenterId == 0 || p.costcenterId == costcenterId)
            )
            .Select(p => new
            {
                id = p.id,
                insertUser = _context.hr_user
                    .Where(u => u.id == p.insertUserId)
                    .Select(u => u.username)
                    .FirstOrDefault() ?? "",
                itemId = p.itemId,
                item = p.item,
                dealer = p.dealer,
                costcenter = _context.acc_CostCenters
                    .Where(c => c.id == p.costcenterId)
                    .Select(c => c.costCenter)
                    .FirstOrDefault(),

                unitPrice = p.unitPrice,
                qty = p.qty,
                total = p.total,
                processDate = p.processDate,
                invoiceNo = p.invoiceNo ?? "",
                isReviewed = p.isreviewed
            })
            .ToList();

        return Json(rows);
    }








    [HttpPost]
    public IActionResult ToggleReview([FromBody] ReviewVM model)
    {
        if (model == null || model.Id == 0)
            return BadRequest("بيانات غير صحيحة");

        // ✅ مراجع أو فتحي فقط
        if (!PermissionViewHelper.CanReview(HttpContext))
            return Forbid("ليس لديك صلاحية");

        var row = _context.Purchases.Find(model.Id);
        if (row == null)
            return NotFound("السجل غير موجود");

        row.isreviewed = model.Review;
        _context.SaveChanges();

        return Ok();
    }








    public class ReviewVM
    {
        public int Id { get; set; }
        public bool Review { get; set; }
    }




    [HttpPost]
    public IActionResult Save([FromBody] PurchaseVM model)
    {
        if (model == null)
            return BadRequest("Model null");

        const int SCREEN_ID = (int)Screens.Purchase;
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

        // =========================
        // 🔐 صلاحيات إضافة / تعديل
        // =========================
        if (model.Id == 0)
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                return Forbid("غير مسموح لك بالإضافة");
        }
        else
        {
            if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                return Forbid("غير مسموح لك بالتعديل");

            // ❌ ممنوع تعديل سجل مراجع
            if (_context.Purchases.Any(x => x.id == model.Id && x.isreviewed == true))
                return BadRequest("لا يمكن تعديل سجل مراجع");
        }

        // =========================
        // 🔐 صلاحية الموقع
        // =========================
        if (!model.CostCenterId.HasValue ||
            !PermissionHelper.CanCostCenter(model.CostCenterId.Value, HttpContext))
            return Forbid("غير مسموح لك على هذا الموقع");

        Purchase p;

        if (model.Id > 0)
        {
            // =========================
            // ✏️ تعديل
            // =========================
            p = _context.Purchases.Find(model.Id);
            if (p == null)
                return NotFound("السجل غير موجود");

            if (p.isreviewed == true)
                return BadRequest("لا يمكن التعديل بعد المراجعة");

            p.lastUpdateDate = DateTime.Now;
            p.lastUpdateUserId = userId;
        }
        else
        {
            // =========================
            // ➕ إضافة
            // =========================
            p = new Purchase
            {
                insertDate = DateTime.Now,
                insertUserId = userId,
                isreviewed = false
            };
            _context.Purchases.Add(p);
        }

        // =========================
        // 🧾 Assign
        // =========================
        p.processDate = model.Date;
        p.costcenterId = model.CostCenterId.Value;
        p.dealerId = model.DealerId.Value;
        p.itemId = model.ItemId.Value;
        p.qty = model.Qty;
        p.unitPrice = model.UnitPrice;
        p.total = model.Qty * model.UnitPrice;
        p.invoiceNo = model.InvoiceNo?.Trim();

        // أسماء العرض (للسرعة في الجريد)
        p.item = _context.Items
            .Where(x => x.Id == model.ItemId.Value)
            .Select(x => x.Name)
            .First();

        p.dealer = _context.Dealers
            .Where(x => x.id == model.DealerId.Value)
            .Select(x => x.Name)
            .First();

        _context.SaveChanges();

        return Ok(model.Id > 0
            ? "تم التعديل بنجاح"
            : "تم الحفظ بنجاح");
    }





    [HttpGet]
    public IActionResult LoadItems()
    {
        var items = _context.Items
            .Select(x => new
            {
                id = x.Id,
                name = x.Name
            })
            .ToList();

        return Json(items);
    }




    [HttpPost]
    public IActionResult Delete(int id)
    {
        // 🔐 صلاحية الحذف
        if (!PermissionHelper.Can((int)Screens.Purchase, "Delete", HttpContext))
            return Forbid("غير مسموح لك بالحذف");

        // ❌ ممنوع حذف سجل مراجع
        if (_context.Purchases.Any(x => x.id == id && x.isreviewed == true))
            return BadRequest("لا يمكن حذف سجل مراجع");

        var row = _context.Purchases.Find(id);
        if (row == null)
            return NotFound("السجل غير موجود");

        _context.Purchases.Remove(row);
        _context.SaveChanges();

        return Ok("تم الحذف بنجاح");
    }







    private void LoadCostCenters()
    {
        ViewBag.CostCenters = _context.acc_CostCenter.ToList();
    }
    







}
