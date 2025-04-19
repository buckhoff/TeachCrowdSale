using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Api.Models;

public class MarketDataModel
{
    [Range(0, double.MaxValue)]
    public decimal CurrentPrice { get; set; }
    [Range(0, double.MaxValue)]
    public decimal MarketCap { get; set; }
    public decimal PriceChange24h { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Volume24h { get; set; }
    [Range(0, int.MaxValue)]
    public int HoldersCount { get; set; }
}