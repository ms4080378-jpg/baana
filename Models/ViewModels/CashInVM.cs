namespace elbanna.Models.ViewModels
{
    public class CashInVM
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public string Custody { get; set; }
        public decimal Balance { get; set; }
        public string Notes { get; set; }

        public int? CostCenterId { get; set; }
        public string CostCenter { get; set; }
    }
}
