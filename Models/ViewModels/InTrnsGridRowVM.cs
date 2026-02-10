namespace elbanna.ViewModels
{
    public class InTrnsGridRowVM
    {
        public int Id { get; set; }

        public string Item { get; set; }
        public string CostCenter { get; set; }

        public DateTime? ProcessDate { get; set; }

        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }

        public string Building { get; set; }
        public string Floor { get; set; }
        public string Unit { get; set; }

    }
}
