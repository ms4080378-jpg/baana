using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.Models.ViewModels;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // ===== Helpers =====
        private SelectList BuildJobsSelectList(int selectedId)
        {
            var jobs = _context.st_job
                .AsNoTracking()
                .OrderBy(x => x.id)
                .Select(x => new { Id = x.id, Name = x.job })
                .ToList();

            return new SelectList(jobs, "Id", "Name", selectedId);
        }

        private int GetDefaultJobId()
        {
            var ids = _context.st_job.AsNoTracking().OrderBy(x => x.id).Select(x => x.id).ToList();
            int defaultJobId = ids.Skip(1).FirstOrDefault();
            if (defaultJobId == 0) defaultJobId = ids.FirstOrDefault();
            return defaultJobId;
        }

        private async Task<UserPermissionsVM> BuildPermissionsVM(int id)
        {
            var screens = await _context.st_screen.AsNoTracking().OrderBy(x => x.id).ToListAsync();
            var costCenters = await _context.acc_CostCenter.AsNoTracking().OrderBy(x => x.id).ToListAsync();

            var savedScreens = await _context.st_userPermission.Where(x => x.userId == id).ToListAsync();
            var savedCC = await _context.st_UserCCPermission.Where(x => x.userId == id).ToListAsync();

            foreach (var s in screens)
                if (!savedScreens.Any(p => p.screen == s.id))
                    _context.st_userPermission.Add(new st_userPermission { userId = id, screen = s.id });

            foreach (var c in costCenters)
                if (!savedCC.Any(p => p.costcenter == c.id))
                    _context.st_UserCCPermission.Add(new st_UserCCPermission { userId = id, costcenter = c.id });

            await _context.SaveChangesAsync();

            savedScreens = await _context.st_userPermission.Where(x => x.userId == id).ToListAsync();
            savedCC = await _context.st_UserCCPermission.Where(x => x.userId == id).ToListAsync();

            return new UserPermissionsVM
            {
                UserId = id,
                Screens = screens.Select(s =>
                {
                    var p = savedScreens.First(x => x.screen == s.id);
                    return new PermissionRowVM
                    {
                        RefId = s.id,
                        Name = s.screen,
                        CanAdd = p.canAdd,
                        CanEdit = p.canEdit,
                        CanDelete = p.canDelete,
                        CanPrint = p.canPrint
                    };
                }).ToList(),

                CostCenters = costCenters.Select(c =>
                {
                    var p = savedCC.First(x => x.costcenter == c.id);
                    return new PermissionRowVM
                    {
                        RefId = c.id,
                        Name = c.costCenter ?? "",
                        CanAdd = p.canAdd,
                        CanEdit = p.canEdit,
                        CanDelete = p.canDelete,
                        CanPrint = p.canPrint
                    };
                }).ToList()
            };
        }

        // ===== Index =====
        public async Task<IActionResult> Index()
        {
            if (!PermissionHelper.CanOpenScreen((int)Screens.Users, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var data = await _context.hr_user
                .AsNoTracking()
                .Select(u => new UsersIndexVM
                {
                    id = u.id,
                    username = u.username,
                    job = u.job
                })
                .ToListAsync();

            return View(data);
        }

        // ===== Form =====
        [HttpGet]
        public IActionResult Form(int id = 0)
        {
            if (!PermissionHelper.CanOpenScreen((int)Screens.Users, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            // جديد = Add ، تعديل = Edit
            if (id == 0)
            {
                if (!PermissionHelper.Can((int)Screens.Users, "Add", HttpContext))
                    return RedirectToAction("AccessDenied", "Auth");
            }
            else
            {
                if (!PermissionHelper.Can((int)Screens.Users, "Edit", HttpContext) &&
                    !PermissionHelper.Can((int)Screens.Users, "Add", HttpContext))
                    return RedirectToAction("AccessDenied", "Auth");
            }

            if (id == 0)
            {
                int defaultJobId = GetDefaultJobId();
                ViewBag.Jobs = BuildJobsSelectList(defaultJobId);

                return View(new UserFormVM
                {
                    jobId = defaultJobId
                });
            }

            var u = _context.hr_user.AsNoTracking().FirstOrDefault(x => x.id == id);
            if (u == null) return NotFound();

            int selectedJobId = u.jobId ?? 0;
            ViewBag.Jobs = BuildJobsSelectList(selectedJobId);

            return View(new UserFormVM
            {
                id = u.id,
                username = u.username ?? "",
                Password = u.password ?? "",
                islogged = u.islogged ?? false,
                jobId = selectedJobId,

                allowSaveDays = u.allowSaveDays ?? 0,
                allowFutureSaveDays = u.allowFutureSaveDays ?? 0,
                allowShowDays = u.allowShowDays ?? 0,
                allowShowOtherData = u.allowShowOtherData ?? false,

                canReview = u.canReview ?? false,
                canPaid = u.canPaid ?? false,
                canUpdateCustody = u.canUpdateCustody ?? false
            });
        }

        // لو في أي روابط قديمة
        [HttpGet]
        public IActionResult Edit(int id) => RedirectToAction(nameof(Form), new { id });

        // ===== Save User =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(UserFormVM m)
        {
            bool isNew = m.id == 0;

            if (!PermissionHelper.Can((int)Screens.Users, "Add", HttpContext))
            {
                TempData["Error"] = "معلش ياصاحبي مش مسموحلك تعمل كدا";
                return RedirectToAction(nameof(Form), new { id = m.id });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Jobs = BuildJobsSelectList(m.jobId);
                return View("Form", m);
            }

            var jobName = _context.st_job.AsNoTracking()
                .Where(x => x.id == m.jobId)
                .Select(x => x.job)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(jobName))
            {
                ModelState.AddModelError(nameof(m.jobId), "يجب اختيار الوظيفة");
                ViewBag.Jobs = BuildJobsSelectList(m.jobId);
                return View("Form", m);
            }

            hr_user u;

            if (isNew)
            {
                u = new hr_user();
                _context.hr_user.Add(u);
            }
            else
            {
                u = _context.hr_user.FirstOrDefault(x => x.id == m.id);
                if (u == null) return NotFound();
            }

            u.username = (m.username ?? "").Trim();
            u.password = (m.Password ?? "").Trim();

            u.jobId = m.jobId;
            u.job = jobName;

            u.islogged = m.islogged;
            u.allowSaveDays = m.allowSaveDays;
            u.allowFutureSaveDays = m.allowFutureSaveDays;
            u.allowShowDays = m.allowShowDays;
            u.allowShowOtherData = m.allowShowOtherData;

            u.canReview = m.canReview;
            u.canPaid = m.canPaid;
            u.canUpdateCustody = m.canUpdateCustody;

            _context.SaveChanges();

            // Seed لأول مرة
            if (isNew)
            {
                foreach (var s in _context.st_screen.AsNoTracking())
                    _context.st_userPermission.Add(new st_userPermission { userId = u.id, screen = s.id });

                foreach (var cc in _context.acc_CostCenter.AsNoTracking())
                    _context.st_UserCCPermission.Add(new st_UserCCPermission { userId = u.id, costcenter = cc.id });

                _context.SaveChanges();
            }

            TempData["Success"] = "تم الحفظ بنجاح";
            return RedirectToAction(nameof(Form), new { id = u.id });
        }

        // ===== Permissions Pages =====
        // ===== Permissions Pages =====
        [HttpGet]
        public async Task<IActionResult> ScreenPermissions(int id)
        {
            if (!PermissionHelper.Can((int)Screens.Users, "Add", HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var vm = await BuildPermissionsVM(id);
            return View(vm); // Views/Users/ScreenPermissions.cshtml
        }

        [HttpGet]
        public async Task<IActionResult> SitePermissions(int id)
        {
            if (!PermissionHelper.Can((int)Screens.Users, "Add", HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            var vm = await BuildPermissionsVM(id);
            return View(vm); // Views/Users/SitePermissions.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermissions(UserPermissionsVM model, string mode)
        {
            if (!PermissionHelper.Can((int)Screens.Users, "Add", HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            mode = (mode ?? "").ToLower();

            model.Screens ??= new List<PermissionRowVM>();
            model.CostCenters ??= new List<PermissionRowVM>();

            // تحميل الموجود
            var existingScreens = await _context.st_userPermission
                .Where(x => x.userId == model.UserId)
                .ToListAsync();

            var existingCC = await _context.st_UserCCPermission
                .Where(x => x.userId == model.UserId)
                .ToListAsync();

            // ✅ لو صفحة الشاشات: حدّث Screens فقط
            if (mode != "sites")
            {
                foreach (var s in model.Screens)
                {
                    var row = existingScreens.FirstOrDefault(x => x.screen == s.RefId);
                    if (row == null)
                    {
                        row = new st_userPermission { userId = model.UserId, screen = s.RefId };
                        _context.st_userPermission.Add(row);
                    }

                    row.canAdd = s.CanAdd;
                    row.canEdit = s.CanEdit;
                    row.canDelete = s.CanDelete;
                    row.canPrint = s.CanPrint;
                }
            }

            // ✅ لو صفحة المواقع: حدّث CostCenters فقط
            if (mode == "sites")
            {
                foreach (var c in model.CostCenters)
                {
                    var row = existingCC.FirstOrDefault(x => x.costcenter == c.RefId);
                    if (row == null)
                    {
                        row = new st_UserCCPermission { userId = model.UserId, costcenter = c.RefId };
                        _context.st_UserCCPermission.Add(row);
                    }

                    row.canAdd = c.CanAdd;
                    row.canEdit = c.CanEdit;
                    row.canDelete = c.CanDelete;
                    row.canPrint = c.CanPrint;
                }
            }

            await _context.SaveChangesAsync();

            // ✅ تحديث Session لو نفس المستخدم الحالي (علشان الصلاحيات تتحدث فورًا)
            if (SessionUser.UserId(HttpContext) == model.UserId)
            {
                var screenPermissions = await _context.st_userPermission
                    .Where(x => x.userId == model.UserId)
                    .ToListAsync();

                HttpContext.Session.SetString(
                    "permissions",
                    Newtonsoft.Json.JsonConvert.SerializeObject(screenPermissions)
                );

                // ✅ مهم: نخزن نفس نوع st_UserCCPermission (مش anonymous)
                var ccPermissions = await _context.st_UserCCPermission
                    .Where(x => x.userId == model.UserId)
                    .Select(x => new st_UserCCPermission
                    {
                        userId = x.userId,
                        costcenter = x.costcenter,
                        canAdd = x.canAdd,
                        canEdit = x.canEdit,
                        canDelete = x.canDelete,
                        canPrint = x.canPrint
                    })
                    .ToListAsync();

                HttpContext.Session.SetString(
                    "cc_permissions",
                    Newtonsoft.Json.JsonConvert.SerializeObject(ccPermissions)
                );
            }

            TempData["Success"] = "تم تعديل الصلاحيات بنجاح";

            return RedirectToAction(
                mode == "sites" ? nameof(SitePermissions) : nameof(ScreenPermissions),
                new { id = model.UserId }
            );
        }


    }

}

