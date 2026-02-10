using System;
using System.Collections.Generic;
using elbanna.Models;

namespace elbanna.Models.ViewModels
{
    public class StockTransferIndexVM
    {
        // ===== Identity =====
        public int Id { get; set; }

        // ===== Filters =====
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // ===== Header =====
        public DateTime Date { get; set; }

        public int? FromCostCenterId { get; set; }
        public string? FromCostCenterName { get; set; }

        public int? ToCostCenterId { get; set; }
        public string? ToCostCenterName { get; set; }

        public int? ItemId { get; set; }
        public string? ItemName { get; set; }

        public decimal Qty { get; set; }

        // ===== Lists =====
        public List<acc_CostCenter> CostCenters { get; set; } = new();
        public List<Item> Items { get; set; } = new();
    }
}
