namespace elbanna.Helpers
{
  
        public enum ScreenNames
        {
            Daily = 1,
            Users = 2,
            Purchase = 5,
           StockTransfer = 6,
           Reports = 7
           // كمّلي حسب جدول st_screen
        }
    public enum Screens
    {
        // =================
        // محاسبة
        // =================
        Daily = 23,                 // اليومية
        Purchase = 17,              // المشتريات
        CashIn = 8,                 // وارد نقدية
        BankTransfer = 91,           // تسجيل تحويلات
        Cheque = 81,                 // شيكات
        ChequeRec = 82,             // تسجيل شيكات
        Balance = 10,               // أرصدة
        CustodyStage=1,              // عهدة مرحله
        // =================
        // مخازن
        // =================
        intrns = 15,               // وارد مخزن
        StoreInventory = 9,         // جرد مخزن
        ItemMovement = 19,          // حركة صنف
        OutTrns = 16,            // صرف من مشروع
        StockTransfer = 6,         // تحويل بين المشروعات

        // =================
        // مستخلصات
        // =================
        Invoice = 20,               // المستخلصات
        PayInvoice = 12,            // مستخلصات المستخلصات
        InvoiceReport = 14,         // كشف مستخلص

        // =================
        // بيانات أساسية
        // =================
        Dealer = 21,                // متعامل جديد
        Item = 25,                  // صنف جديد
        ItemPurchase = 84,    // مشتريات المقاولين

        // =================
        // تقارير
        // =================
        PurchaseReports = 3,        // تقارير مشتريات
        ProjectCashReport = 4,      // كشف خزينة مشروع
        ProjectStatus = 90,         // الموقف التنفيذي للمشروع

        // =================
        // إدارة النظام
        // =================
        Users = 24,                 // المستخدمين
        UserLog = 26                // سجل المستخدمين
    }

}
