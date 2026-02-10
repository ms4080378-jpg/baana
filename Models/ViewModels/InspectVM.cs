using Microsoft.AspNetCore.Mvc.Rendering;

namespace YourProject.Models.ViewModels
{
    public class InspectVM
    {
        public int Id { get; set; }

        public int? ItemId { get; set; }
        public int? CostCenterId { get; set; }

        public string Building { get; set; }
        public string Floor { get; set; }
        public string Unit { get; set; }

        public decimal? Qty { get; set; }
        public List<pr_Inspect> List { get; set; } = new();
        public List<pr_Inspect> InspectList { get; set; }

        // ⬇️ نضيف دي
        public Dictionary<int, string> ItemNames { get; set; } = new();

        public List<SelectListItem> Items { get; set; }
        public List<SelectListItem> CostCenters { get; set; }

    }
}
