using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Api.Models;

public class SupplyModel
{
    [Range(0, double.MaxValue)]
    public decimal TotalSupply { get; set; }
    [Range(0, double.MaxValue)]
    public decimal CirculatingSupply { get; set; }
    [PercentageRange]
    public decimal PercentCirculating { get; set; }
}