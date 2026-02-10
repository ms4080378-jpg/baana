namespace elbanna.Models
{
    public class ConInvoiceDetail
    {

        public int id { get; set; }

        public int invoiceId { get; set; }

        public int statementId { get; set; }   // البيان
        public decimal value { get; set; }      // قيمة المعامل
        public string factor { get; set; }      // % أو *
        public bool isCredit { get; set; }      // ترد / مراجع
    }

}
