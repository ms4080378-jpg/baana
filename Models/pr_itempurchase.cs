namespace YourProject.Models
{
    public class pr_itempurchase
    {
        public int id { get; set; }

        public DateTime? processDate { get; set; }

        public decimal? qty { get; set; }
        public decimal? unitPrice { get; set; }
        public decimal? total { get; set; }

        public bool? isreviewed { get; set; }

        public DateTime? insertDate { get; set; }
        public DateTime? lastUpdateDate { get; set; }

	        // ✅ Audit Users (عشان عمود المستخدم يشتغل)
	        public int? insertUserId { get; set; }
	        public int? lastUpdateUserId { get; set; }

        public int? itemId { get; set; }
        public string item { get; set; }

        public int? dealerId { get; set; }
        public string dealer { get; set; }

        public int? costcenterId { get; set; }   // ✅ التعديل الحاسم
        public string costcenter { get; set; }

        public string building { get; set; }
        public string floor { get; set; }
        public string unit { get; set; }
    }

}