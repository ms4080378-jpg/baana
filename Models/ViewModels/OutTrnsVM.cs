using System;

namespace elbanna.Models.ViewModels
{
    public class OutTrnsVM
    {
        public int Id { get; set; }

        // التاريخ
        public DateTime ProcessDate { get; set; }

        // الموقع
        public int CostCenterId { get; set; }
        public string? CostCenter { get; set; }

        // الصنف
        public int ItemId { get; set; }
        public string? Item { get; set; }

        // الكمية
        public decimal Qty { get; set; }

        // ملاحظات
        public string? Notes { get; set; }

        // للفترة (عرض القائمة)
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? Date { get; internal set; }
    }
}
