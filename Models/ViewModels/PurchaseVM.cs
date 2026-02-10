namespace elbanna.Models.ViewModels
{
    public class PurchaseVM
    {
        // 🔹 Filters / Header
        public DateTime Date { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }

        public int? CostCenterId { get; set; }
        public int? DealerId { get; set; }
        public int? ItemId { get; set; }
        public List<Item> Items { get; set; }
        
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }

        // 🔹 Dropdowns
        public List<acc_CostCenter> CostCenters { get; set; } = new();
        public List<Dealer> Dealers { get; set; } = new();

    }
}
