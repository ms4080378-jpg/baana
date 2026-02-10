namespace elbanna.Models
{
    public class pr_outtrns
    {
        public int id { get; set; }

	        // ✅ Audit (عشان عمود المستخدم يشتغل)
	        public DateTime? insertDate { get; set; }
	        public DateTime? lastUpdateDate { get; set; }
	        public int? insertUserId { get; set; }
	        public int? lastUpdateUserId { get; set; }
	        public bool? isreviewed { get; set; }

        public string item { get; set; }
        public int? itemid { get; set; }

        public string costcenter { get; set; }
        public int? costcenterid { get; set; }

        public DateTime? processDate { get; set; }
        public decimal? qty { get; set; }
        public string notes { get; set; }
    }

}
