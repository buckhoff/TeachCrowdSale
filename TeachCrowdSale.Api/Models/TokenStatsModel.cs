using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Api.Models;

public class TokenStatsModel
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
    [Range(0, double.MaxValue)]
    public decimal BurnedTokens { get; set; }
    [Range(0, double.MaxValue)]
    public decimal StakedTokens { get; set; }
    [Range(0, double.MaxValue)]
    public decimal LiquidityTokens { get; set; }
}