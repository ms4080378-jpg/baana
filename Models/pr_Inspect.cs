namespace YourProject.Models
{
    public class pr_Inspect
    {
        public int id { get; set; }

        public int? itemId { get; set; }

        public string building { get; set; }
        public string floor { get; set; }
        public string unit { get; set; }

        public decimal? qty { get; set; }          // ✅

        public string notes { get; set; }

        public int? costcenterId { get; set; }     // ✅
        public string costcenter { get; set; }
        public DateTime? lastUpdateDate { get; set; }
        public int? lastUpdateUserId { get; set; }
    }

}