using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models;

public class TokenInfo
{
    public string Name { get; set; }
    public string Symbol { get; set; }
    [Range(0, 18)]
    public int Decimals { get; set; }
    [Range(0, double.MaxValue)]
    public decimal TotalSupply { get; set; }
    [Range(0, double.MaxValue)]
    public decimal CirculatingSupply { get; set; }
    [Range(0, double.MaxValue)]
    public decimal CurrentPrice { get; set; }
    [Range(0, double.MaxValue)]
    public decimal MarketCap { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Volume24h { get; set; }
    [Range(0, int.MaxValue)]
    public int HoldersCount { get; set; }
}