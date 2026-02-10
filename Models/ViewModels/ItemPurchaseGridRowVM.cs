namespace YourProject.ViewModels
{
    public class ItemPurchaseGridRowVM
    {
        public int Id { get; set; }

        public string Dealer { get; set; }
        public string Item { get; set; }
        public string CostCenter { get; set; }

        public DateTime? ProcessDate { get; set; }

        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }

        public string Building { get; set; }
        public string Floor { get; set; }
        public string Unit { get; set; }

        public bool IsReviewed { get; set; }

        // 👇 اسم المستخدم اللي حفظ السجل
        public string UserName { get; set; } = "";
    }
}