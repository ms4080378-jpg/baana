using Microsoft.AspNetCore.Mvc.Rendering;

namespace elbanna.ViewModels
{
    public class InTrnsFilterVM
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int? CostCenterId { get; set; }

        public List<SelectListItem> CostCenters { get; set; } = new();
    }
}
