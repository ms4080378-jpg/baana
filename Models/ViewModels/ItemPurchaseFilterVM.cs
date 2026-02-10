using Microsoft.AspNetCore.Mvc.Rendering;

namespace YourProject.ViewModels
{
    public class ItemPurchaseFilterVM
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int? CostCenterId { get; set; }
        public int? DealerId { get; set; }
        public int? ItemId { get; set; }

        public string Building { get; set; }
        public string Floor { get; set; }
        public string Unit { get; set; }

        /* Dropdown Data */
        public List<SelectListItem> CostCenters { get; set; } = new();
        public List<SelectListItem> Dealers { get; set; } = new();
        public List<SelectListItem> Items { get; set; } = new();
    }
}