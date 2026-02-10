using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using Microsoft.AspNetCore.Mvc;

namespace elbanna.Controllers
{
    public class DealerController : Controller
    {
        private readonly AppDbContext _context;

        // ✅ غيرها لو اسم شاشة المتعاملين مختلف
        private const int SCREEN_ID = (int)Screens.Dealer;

        public DealerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ❌ منع فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            return View();
        }

        [HttpGet]
        public IActionResult GetList()
        {
            // ❌ منع سحب الداتا بدون صلاحية فتح الشاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            var data = _context.Set<Dealer>()
                .OrderByDescending(x => x.id)
                .Select(x => new
                {
                    x.id,
                    x.dealer,
                    x.code,
                    x.nationalId,
                    x.isStopped,
                    x.isBank
                }).ToList();

            return Json(data);
        }

        [HttpPost]
        public IActionResult Save([FromBody] Dealer model)
        {
            // ❌ فتح شاشة
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            // ✅ صلاحية إضافة
            if (!PermissionHelper.Can(SCREEN_ID, "Add", HttpContext))
                return Forbid("غير مسموح لك بالحفظ");

            if (model == null)
                return BadRequest("بيانات غير صالحة");

            if (string.IsNullOrWhiteSpace(model.dealer))
                return BadRequest("البيان مطلوب");

            _context.Add(model);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult Update([FromBody] Dealer model)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            // ✅ صلاحية تعديل
            if (!PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext))
                return Forbid("غير مسموح لك بالتعديل");

            if (model == null)
                return BadRequest("بيانات غير صالحة");

            var item = _context.Set<Dealer>().Find(model.id);
            if (item == null) return NotFound();

            item.dealer = model.dealer;
            item.code = model.code;
            item.nationalId = model.nationalId;
            item.isStopped = model.isStopped;
            item.isBank = model.isBank;

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            // ✅ صلاحية حذف
            if (!PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح لك بالحذف");

            var item = _context.Set<Dealer>().Find(id);
            if (item == null) return NotFound();

            _context.Remove(item);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IActionResult GetDealerInfo(int id)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid();

            var dealer = _context.Dealers
                .Where(x => x.id == id)
                .Select(x => new
                {
                    x.id,
                    code = x.code,
                    nationalId = x.nationalId
                })
                .FirstOrDefault();

            if (dealer == null)
                return NotFound();

            return Json(dealer);
        }
    }
}
