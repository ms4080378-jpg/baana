using elbanna.Models;

namespace elbanna.ViewModels
{
    public class BankTransferVM
    {
        // Filters
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int? CostCenterId { get; set; }
        public List<acc_CreditAccount> CreditAccounts { get; set; }

        // Form
        public int Id { get; set; }
        public DateTime ProcessDate { get; set; }
        public int? DealerId { get; set; }
        public int? BankId { get; set; }
        public decimal Total { get; set; }
        public string Notes { get; set; }
        public string FromAcc { get; set; }
        public string ToAcc { get; set; }
        public string InvoiceCode { get; set; }

        // Lists
        public List<acc_CostCenter> CostCenters { get; set; }
        public List<Dealer> Dealers { get; set; }
        public List<Dealer> Banks { get; set; }

        // Grid
        public List<acc_BankTransfer> List { get; set; }
    }
}
//تسجيل تحويلات