using Microsoft.AspNetCore.Mvc.Rendering;

namespace elbanna.ViewModels
{
    public class BalanceSheetVM
    {
        public int? CostCenterId { get; set; }

        public List<SelectListItem> CostCenters { get; set; } = new();

        public List<BalanceSheetRowVM> Results { get; set; } = new();
    }
}
//تسجيل تحويلات