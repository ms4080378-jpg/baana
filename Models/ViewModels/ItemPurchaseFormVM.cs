using Microsoft.AspNetCore.Mvc.Rendering;

namespace YourProject.ViewModels
{
    public class ItemPurchaseFormVM
    {
        public int Id { get; set; }

        public DateTime? ProcessDate { get; set; }

        public int? CostCenterId { get; set; }
        public string CostCenter { get; set; }

        public int? DealerId { get; set; }
        public string Dealer { get; set; }

        public int? ItemId { get; set; }
        public string Item { get; set; }

        public string Building { get; set; }
        public string Floor { get; set; }
        public string Unit { get; set; }

        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Total { get; set; }
        public bool IsReviewed { get; set; }


        /* Dropdown Data */
        public List<SelectListItem> CostCenters { get; set; } = new();
        public List<SelectListItem> Dealers { get; set; } = new();
        public List<SelectListItem> Items { get; set; } = new();

        public List<string> Buildings { get; set; } = new();
        public List<string> Floors { get; set; } = new();
        public List<string> Units { get; set; } = new();
    }
}