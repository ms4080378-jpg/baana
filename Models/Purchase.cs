using elbanna.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
[Table("pr_purchase")]
public class Purchase
{
    [Key]
    public int id { get; set; }

    public string? item { get; set; }
    public int? itemId { get; set; }

    public string? dealer { get; set; }
    public int? dealerId { get; set; }

    [NotMapped]
    public string? costcenter { get; set; }

    public int? costcenterId { get; set; }

    [ForeignKey(nameof(costcenterId))]
    public acc_CostCenter CostCenter { get; set; }

    public DateTime? processDate { get; set; }

    public decimal? qty { get; set; }
    public decimal? unitPrice { get; set; }
    public decimal? total { get; set; }

    public bool? isreviewed { get; set; }

    [NotMapped]
    public int? costcenterid { get; set; }

    public int? insertUserId { get; set; }
    public DateTime? insertDate { get; set; }

    public DateTime? lastUpdateDate { get; set; }
    public int? lastUpdateUserId { get; set; }

    public string? invoiceNo { get; set; }
}
