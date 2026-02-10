namespace elbanna.Models.ViewModels
{
    public class PayInvoiceVM
    {
        /* ===============================
         * بيانات السجل (Header)
         * =============================== */
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        public string InvoiceCode { get; set; }

        public List<con_payInvoice> List { get; set; }

        /* ===============================
         * القيم المحاسبية
         * =============================== */
        public decimal Total { get; set; }      // إجمالي الأعمال
        public decimal Debit { get; set; }      // إجمالي الاستقطاعات
        public decimal Paid { get; set; }       // مدفوع سابقًا
        public decimal Amount { get; set; }     // المبلغ الحالي
        public decimal Net { get; set; }         // الصافي
        public decimal Remain { get; set; }      // الباقي


        /* ===============================
         * بيانات إضافية
         * =============================== */
        public string Notes { get; set; }


        /* ===============================
         * الصلاحيات
         * =============================== */
        public bool CanReview { get; set; }
        public bool CanPaid { get; set; }


        /* ===============================
         * القوائم (DropDowns / Grid)
         * =============================== */
        public List<acc_CostCenter> CostCenters { get; set; }

        // أرقام المستخلصات حسب الموقع
        public List<string> InvoiceCodes { get; set; }

        // جدول العرض

    }
}
