using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models;

/// <summary>
/// Supply metrics model for web consumption
/// Consolidates SupplyModel and TokenStatsModel functionality
/// Maps from SupplySnapshot entity
/// </summary>
public class SupplyMetricsModel
{
    [Range(0, double.MaxValue)]
    public decimal MaxSupply { get; set; } = 5_000_000_000;

    [Range(0, double.MaxValue)]
    public decimal CurrentSupply { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalSupply { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CirculatingSupply { get; set; }

    [Range(0, double.MaxValue)]
    public decimal LockedSupply { get; set; }

    [Range(0, double.MaxValue)]
    public decimal BurnedSupply { get; set; }

    [Range(0, 100)]
    public decimal CirculatingPercent { get; set; }

    [Range(0, 100)]
    public decimal LockedPercent { get; set; }

    [Range(0, 100)]
    public decimal BurnedPercent { get; set; }

    [Range(0, 100)]
    public decimal PercentCirculating { get; set; }

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