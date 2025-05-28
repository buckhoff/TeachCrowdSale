using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Response;

public class TokenInfoModel
{
    [Range(0, double.MaxValue)]
    public decimal TotalSupply { get; set; }
    [Range(0, double.MaxValue)]
    public decimal CirculatingSupply { get; set; }
    [Range(0, double.MaxValue)]
    public decimal CurrentPrice { get; set; }
    [Range(0, double.MaxValue)]
    public decimal MarketCap { get; set; }
    [Range(0, int.MaxValue)]
    public int HoldersCount { get; set; }
}