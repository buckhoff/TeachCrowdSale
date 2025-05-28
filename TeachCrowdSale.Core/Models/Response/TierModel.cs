using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response;

public class TierModel
{
    [Range(0, int.MaxValue)]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Range(0.00000001, double.MaxValue)]
    public decimal Price { get; set; }
    [Range(0, double.MaxValue)]
    public decimal MinPurchase { get; set; }
    [Range(0, double.MaxValue)]
    public decimal MaxPurchase { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TotalAllocation { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Sold { get; set; }
    [PercentageRange]
    public int VestingTGE { get; set; }
    [Range(0, 60)]
    public int VestingMonths { get; set; }
    public bool IsActive { get; set; }
    public DateTime EndTime { get; set; }
}