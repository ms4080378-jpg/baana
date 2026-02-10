namespace elbanna.ViewModels
{
    public class InTrnsFormVM
    {
        public int Id { get; set; }

        public DateTime ProcessDate { get; set; }

        public int CostCenterId { get; set; }
        public string CostCenter { get; set; }

        public string Item { get; set; }

        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
