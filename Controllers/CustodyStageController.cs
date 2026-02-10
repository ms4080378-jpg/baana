using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class CustodyStageController : Controller
    {
        private readonly AppDbContext _context;

        // 🔒 User افتراضي موجود فعليًا في dbo.hr_user
        private const int SYSTEM_USER_ID = 13;
        // 👆 غيّر الرقم لو الـ id عندك مختلف

        public CustodyStageController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // Index
        // =========================
        public IActionResult Index()
        {
            if (!PermissionViewHelper.CanOpen(HttpContext, (int)Screens.CustodyStage))
                return RedirectToAction("AccessDenied", "Auth");
            var vm = new CustodyStageVM
            {
                CustodyDate = DateTime.Today,

                // ⚠️ العهدة = Credit Account (FK حقيقي)
                Custodies = _context.acc_CreditAccount
                    .AsNoTracking()
                    .Where(x => !string.IsNullOrWhiteSpace(x.creditAcc))
                    .Select(x => new acc_custody
                    {
                        id = x.id,               // FK الصحيح
                        custody = x.creditAcc    // الاسم المعروض
                    })
                    .OrderBy(x => x.custody)
                    .ToList()
            };

            return View(vm);
        }

        // =========================
        // Get List
        // =========================
        [HttpGet]
        public IActionResult GetList(DateTime date, int? custodyId)
        {
            if (!PermissionViewHelper.CanOpen(HttpContext, (int)Screens.CustodyStage))
                return Forbid();
            var userId = SYSTEM_USER_ID; // لاحقًا من Session
            var allowOther = true;       // Shared.allowShowOtherData

            var query = _context.acc_custody
                .AsNoTracking()
                .Where(x =>
                    x.date.HasValue &&
                    x.date.Value.Date == date.Date &&
                    (allowOther ||
                     x.insertUserId == userId ||
                     x.lastUpdateUserId == userId)
                );

            if (custodyId.HasValue)
                query = query.Where(x => x.custodyId == custodyId);

            return Json(query.OrderByDescending(x => x.id).ToList());
        }


        // =========================
        // Save
        // =========================
        [HttpPost]
        public IActionResult Save([FromBody] CustodyStageVM model)
        {
            if (model == null)
                return BadRequest("بيانات غير صحيحة");

            if (model.Id == 0)
            {
                if (!PermissionViewHelper.CanAdd(HttpContext, (int)Screens.CustodyStage))
                    return Forbid("غير مسموح لك بالإضافة");
            }
            else
            {
                if (!PermissionViewHelper.CanEdit(HttpContext, (int)Screens.CustodyStage))
                    return Forbid("غير مسموح لك بالتعديل");
            }

            if (model.CustodyId <= 0)
                return BadRequest("يجب اختيار العهدة");

            if (model.CustodyDate == default)
                return BadRequest("يجب إدخال التاريخ");

            if (model.Balance <= 0)
                return BadRequest("المبلغ غير صحيح");

            acc_custody row;
            if (model.Id == 0)
            {
                row = new acc_custody
                {
                    insertDate = DateTime.Now,
                    insertUserId = SYSTEM_USER_ID,      // FK ✔
                    lastUpdateUserId = SYSTEM_USER_ID,  // FK ✔
                    isReviewed = false
                };

                _context.acc_custody.Add(row);
            }

            else
            {
                row = _context.acc_custody.FirstOrDefault(x => x.id == model.Id);
                if (row == null)
                    return NotFound();

                if (row.isReviewed == true)
                    return BadRequest("السجل تمت مراجعته ولا يمكن تعديله");

                row.lastUpdateDate = DateTime.Now;
                row.lastUpdateUserId = SYSTEM_USER_ID;
            }


            // تعيين البيانات
            row.custodyId = model.CustodyId;   // FK → acc_CreditAccount.id
            row.custody = model.Custody;
            row.date = model.CustodyDate;
            row.balance = model.Balance;
            row.notes = model.Notes;

            _context.SaveChanges();

            return Json(true);
        }

        // =========================
        // Delete
        // =========================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!PermissionViewHelper.CanDelete(HttpContext, (int)Screens.CustodyStage))
                return Forbid("غير مسموح لك بالحذف");

            var row = _context.acc_custody.Find(id);
            if (row != null && row.isReviewed != true)
            {
                _context.acc_custody.Remove(row);
                _context.SaveChanges();
            }
            return Json(true);
        }

        // =========================
        // Toggle Review
        // =========================
        [HttpPost]
        public IActionResult ToggleReview(int id, bool force = false)
        {
            // ✅ مراجع أو فتحي فقط
            if (!PermissionViewHelper.CanReview(HttpContext))
                return Forbid("ليس لديك صلاحية");

            var row = _context.acc_custody.Find(id);
            if (row == null)
                return NotFound();

            if (row.isReviewed == true)
            {
                if (!force)
                    return Json(new { needConfirm = true });

                row.isReviewed = false;
            }
            else
            {
                row.isReviewed = true;
            }

            _context.SaveChanges();

            return Json(new
            {
                needConfirm = false,
                isReviewed = row.isReviewed
            });
        }

    }
}
