using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Tokenomics;

/// <summary>
/// Token metrics model for web consumption
/// Maps from TokenMetricsSnapshot entity
/// </summary>
public class TokenMetricsModel
{
    [Range(0, double.MaxValue)]
    public decimal CurrentPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MarketCap { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Volume24h { get; set; }

    public decimal PriceChange24h { get; set; }

    public decimal PriceChangePercent24h { get; set; }

    [Range(0, double.MaxValue)]
    public decimal High24h { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Low24h { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalSupply { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CirculatingSupply { get; set; }

    [Range(0, int.MaxValue)]
    public int HoldersCount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalValueLocked { get; set; }

    public DateTime LastUpdated { get; set; }
}