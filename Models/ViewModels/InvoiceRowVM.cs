namespace elbanna.Models.ViewModels
{
    public class InvoiceRowVM
    {
        public int id { get; set; }
        public string? invoiceCode { get; set; }
        public string? invoiceType { get; set; }
        public int? payForId { get; set; }
        public string? payFor { get; set; }
        public string? factor { get; set; }
        public decimal? factorValue { get; set; }
        public decimal? balance { get; set; }
        public decimal? net { get; set; }
        public DateTime? date { get; set; }
        public int? costcenterId { get; set; }
        public string? costcenter { get; set; }
        public bool? isRefund { get; set; }
        public bool? isReviewed { get; set; }
        public int? lastUpdateUserId { get; set; }
    }
}
