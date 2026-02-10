using elbanna.Models;

namespace elbanna.ViewModels
{
    public class InvoiceIndexVM
    {
        public bool ShowList { get; set; }
        public List<PayForVM> PayFors { get; set; }
        public bool CanReview { get; set; }  // ✅ جديد

        public ConInvoice Invoice { get; set; } = new ConInvoice();
        public List<ConInvoice> List { get; set; } = new List<ConInvoice>();
        public List<acc_CostCenter> CostCenters { get; set; }

        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int? SelectedId { get; set; }

        public int? CostCenterId { get; set; }


    }
}
