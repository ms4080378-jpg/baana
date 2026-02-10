using DocumentFormat.OpenXml.Drawing;
using elbanna.Data;
using elbanna.Helpers;
using elbanna.Models;
using elbanna.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

using System.Linq;

public class ReportsController : Controller
{
    private readonly AppDbContext _db;
    private const int SCREEN_ID = (int)Screens.Balance; // شاشة الأرصدة

    public ReportsController(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // شاشة الاختيار فقط
    // =========================
    public IActionResult BalanceSheet()
    {
        if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
            return RedirectToAction("AccessDenied", "Auth");

        var vm = new BalanceSheetVM
        {
            CostCenters = _db.acc_CostCenters
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.costCenter
                })
                .ToList()
        };

        return View(vm);
    }

    // =========================
    // طباعة
    // =========================
    public IActionResult BalanceSheetPrint(int costCenterId)
    {
        if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
            return RedirectToAction("AccessDenied", "Auth");

        // الطباعة تعتبر Print
        if (!PermissionHelper.Can(SCREEN_ID, "Print", HttpContext))
            return Forbid("غير مسموح لك بالطباعة");

        var vm = GetBalanceSheetVM(costCenterId);
        return View("BalanceSheetPrint", vm);
    }

    // =========================
    // PDF
    // =========================
    public IActionResult BalanceSheetPdf(int costCenterId)
    {
        if (!PermissionHelper.CanOpenScreen(SCREEN_ID, HttpContext))
            return RedirectToAction("AccessDenied", "Auth");

        if (!PermissionHelper.Can(SCREEN_ID, "Print", HttpContext))
            return Forbid("غير مسموح لك بالطباعة");

        var vm = GetBalanceSheetVM(costCenterId);

        return new ViewAsPdf("BalanceSheetPrint", vm)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
        };
    }




    // =========================
    // Logic مشترك (زي الديسكتوب)
    // =========================
    private BalanceSheetVM GetBalanceSheetVM(int costCenterId)
    {
        var vm = new BalanceSheetVM();

        var purchases = _db.Purchases
            .AsNoTracking()
            .Where(p => p.costcenterId == costCenterId && p.total != null)
            .Select(p => new
            {
                p.dealer,
                p.costcenter,
                balance = p.total.Value
            })
            .ToList();

        var daily = _db.acc_Dailies
            .AsNoTracking()
            .Where(d => d.costcenterId == costCenterId && d.net != null)
            .Select(d => new
            {
                d.dealer,
                d.costcenter,
                balance = d.net.Value * -1
            })
            .ToList();

        vm.Results = purchases
            .Concat(daily)
            .GroupBy(x => new { x.dealer, x.costcenter })
            .Select(g => new BalanceSheetRowVM
            {
                dealer = g.Key.dealer,
                costcenter = g.Key.costcenter,
                balance = g.Sum(x => x.balance)
            })
            .ToList();

        return vm;
    }
}
