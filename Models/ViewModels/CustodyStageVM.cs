using elbanna.Models;

namespace elbanna.ViewModels
{
    public class CustodyStageVM
    {
        public int Id { get; set; }
        public int CustodyId { get; set; }   // 👈 FK

        public string Custody { get; set; }

        public DateTime CustodyDate { get; set; }
        public decimal Balance { get; set; }
        public string Notes { get; set; }

        // ✅ لازم تكون موجودة
        public List<acc_custody> Custodies { get; set; }
    }
}
