using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models.ViewModels;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;

namespace elbanna.Controllers
{
    public class ProjectStatusController : Controller
    {
        private readonly AppDbContext _context;
        private const int SCREEN_ID = (int)Screens.ProjectStatus;

        public ProjectStatusController(AppDbContext context)
        {
            _context = context;
        }

        // ==============================
        // الصفحة الرئيسية
        // ==============================
        public IActionResult Index()
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.CostCenters = _context.acc_CostCenters.ToList();
            return View();
        }

        // ==============================
        // تحميل الجدول / PDF داخل الصفحة (Ajax)
        // ==============================
        [HttpPost]
        public IActionResult Load(int CostCenterId, string mode)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return Forbid("غير مسموح");

            // التحميل/العرض مرتبط بـ Print
            if (!PermissionHelper.Can(SCREEN_ID, "Print", HttpContext) &&
                !PermissionHelper.Can(SCREEN_ID, "Add", HttpContext) &&
                !PermissionHelper.Can(SCREEN_ID, "Edit", HttpContext) &&
                !PermissionHelper.Can(SCREEN_ID, "Delete", HttpContext))
                return Forbid("غير مسموح");

            var vm = GetProjectStatusData(CostCenterId);

            if (mode == "pdf")
                return PartialView("_ProjectStatusPdf", vm);

            return PartialView("_ProjectStatusTable", vm);
        }

        // ==============================
        // PDF كامل (Rotativa)
        // ==============================
        public IActionResult Pdf(int costCenterId)
        {
            if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
                return RedirectToAction("AccessDenied", "Auth");

            if (!PermissionHelper.Can(SCREEN_ID, "Print", HttpContext))
                return Forbid("غير مسموح لك بالطباعة");

            var vm = GetProjectStatusData(costCenterId);

            return new ViewAsPdf("Pdf", vm)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

        // ==============================
        // نفس getItems في WinForms
        // ==============================
        private ProjectStatusPageVM GetProjectStatusData(int costCenterId)
        {
            var rows = GetProjectStatusRows(costCenterId);

            var floors = rows
                .SelectMany(r => r.Floors.Keys)
                .Distinct()
                .ToList();

            return new ProjectStatusPageVM
            {
                Floors = floors,
                Rows = rows
            };
        }

        // ==============================
        // بناء الصفوف + الأعمدة الديناميك
        // ==============================
        private List<ProjectStatusRowVM> GetProjectStatusRows(int costCenterId)
        {
            var data = _context.pr_itempurchases
                .Where(x => x.costcenterId == costCenterId)
                .ToList();

            var result = new List<ProjectStatusRowVM>();

            var groups = data.GroupBy(x => new
            {
                x.building,
                x.itemId,
                x.item,
                x.dealer
            });

            foreach (var g in groups)
            {
                var row = new ProjectStatusRowVM
                {
                    BuildingNo = g.Key.building,
                    Item = g.Key.item,
                    Dealer = g.Key.dealer
                };

                decimal total = 0;

                foreach (var f in g.GroupBy(x => x.floor))
                {
                    var qty = f.Sum(x => x.qty ?? 0);
                    row.Floors[f.Key] = qty;
                    total += qty;
                }

                row.Total = total;

                row.Required = _context.pr_Inspects
     .Where(x => x.costcenterId == costCenterId)
     .Where(x => x.building == g.Key.building)
     .Where(x => x.itemId == g.Key.itemId)
     .Sum(x => x.qty) ?? 0;


                result.Add(row);
            }

            return result;
        }
    }
}
