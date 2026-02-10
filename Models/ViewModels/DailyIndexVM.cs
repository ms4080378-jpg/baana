using System;
using System.Collections.Generic;
using elbanna.Models;

namespace elbanna.ViewModels
{
    public class DailyIndexVM
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? Date { get; set; }
        public int CostCenterId { get; set; }


        public decimal? Total { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Net { get; set; }
        public string? FromAccName { get; set; }    // ✔ من عهدة
        public string? ToAccName { get; set; }      // ✔ إلى عهدة
        public string? Dealer { get; set; }
        public int? FromAccId { get; set; }


        public string? DealerName { get; set; }
        public string? DealerCode { get; set; }


        public string? Notes { get; set; }

        public string? CostCenterName { get; set; }
        public int? DealerId { get; set; }

        public string? NationalId { get; set; }


        public int? ToAccId { get; set; }
        public string? Receipt { get; set; }        // ✔ الإيصال
                                                    // ✔ الموقع

        public string? FromAcc { get; set; }
        public string? ToAcc { get; set; }

        public int Id { get; set; }
        public List<Dealer> Dealers { get; set; } = new();
        public List<acc_CostCenter> CostCenters { get; set; } = new();
        public List<acc_CreditAccount> CreditAccounts { get; set; } = new();


    }
}
