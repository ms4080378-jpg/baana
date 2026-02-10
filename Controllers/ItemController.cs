using elbanna.Data;
using elbanna.Helpers;
using Microsoft.AspNetCore.Mvc;

public class ItemController : Controller
{
    private readonly AppDbContext _context;

    // ✅ غيرها لو اسم الشاشة مختلف عندك
    private const int SCREEN_ID = (int)Screens.Item;

    public ItemController(AppDbContext context)
    {
        _context = context;
    }

    // =========================
    // فتح الشاشة
    // =========================
    public IActionResult Index()
    {
        // ❌ منع فتح الشاشة
        if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
            return RedirectToAction("AccessDenied", "Auth");

        ViewBag.ShowTable = false;
        return View(new List<Item>());
    }

    // =========================
    // عرض القائمة
    // =========================
    [HttpPost]
    public IActionResult ShowList()
    {
        // ❌ منع فتح الشاشة
        if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
            return RedirectToAction("AccessDenied", "Auth");

        ViewBag.ShowTable = true;
        return View("Index", _context.Items.ToList());
    }

    // =========================
    // حفظ (Add / Edit)
    // =========================
    [HttpPost]
    public IActionResult Save(Item model)
    {
        // ❌ منع فتح الشاشة
        if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
            return RedirectToAction("AccessDenied", "Auth");

        if (model == null)
            return BadRequest("بيانات غير صالحة");

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

        if (string.IsNullOrWhiteSpace(model.Name))
            return BadRequest("اسم الصنف مطلوب");

        if (model.Id == 0)
            _context.Items.Add(model);
        else
            _context.Items.Update(model);

        _context.SaveChanges();

        ViewBag.ShowTable = true;
        return View("Index", _context.Items.ToList());
    }

    // =========================
    // حذف
    // =========================
    [HttpPost]
    public IActionResult Delete(int id)
    {
        // ❌ منع فتح الشاشة
        if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
            return RedirectToAction("AccessDenied", "Auth");

        if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
            return Forbid("غير مسموح لك بالحذف");

        var item = _context.Items.Find(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            _context.SaveChanges();
        }

        ViewBag.ShowTable = true;
        return View("Index", _context.Items.ToList());
    }
}
