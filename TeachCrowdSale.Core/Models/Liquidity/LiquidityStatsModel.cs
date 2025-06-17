using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Web display model for liquidity statistics and analytics
    /// Maps from LiquidityStatsResponse for web layer consumption
    /// </summary>
    public class LiquidityStatsModel
    {
        // Overall market statistics
        [Display(Name = "Total Value Locked")]
        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Display(Name = "24h Volume")]
        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        [Display(Name = "7d Volume")]
        [Range(0, double.MaxValue)]
        public decimal Volume7d { get; set; }

        [Display(Name = "30d Volume")]
        [Range(0, double.MaxValue)]
        public decimal Volume30d { get; set; }

        [Display(Name = "Total Pools")]
        public int TotalPools { get; set; }

        [Display(Name = "Active Pools")]
        public int ActivePools { get; set; }

        [Display(Name = "Total Users")]
        public int TotalUsers { get; set; }

        [Display(Name = "Active Users")]
        public int ActiveUsers { get; set; }

        // TEACH token specific statistics
        [Display(Name = "TEACH Total Liquidity")]
        public decimal TeachTotalLiquidity { get; set; }

        [Display(Name = "TEACH Pool Count")]
        public int TeachPoolCount { get; set; }

        [Display(Name = "TEACH Average APY")]
        public decimal TeachAverageAPY { get; set; }

        [Display(Name = "TEACH Price")]
        public decimal TeachCurrentPrice { get; set; }

        [Display(Name = "TEACH 24h Change")]
        public decimal TeachPriceChange24h { get; set; }

        [Display(Name = "TEACH Volume 24h")]
        public decimal TeachVolume24h { get; set; }

        // Top performing pools
        [Display(Name = "Highest APY Pool")]
        public LiquidityPoolModel? HighestAPYPool { get; set; }

        [Display(Name = "Highest TVL Pool")]
        public LiquidityPoolModel? HighestTVLPool { get; set; }

        [Display(Name = "Highest Volume Pool")]
        public LiquidityPoolModel? HighestVolumePool { get; set; }

        // DEX distribution
        public Dictionary<string, decimal> DexTVLDistribution { get; set; } = new();
        public Dictionary<string, int> DexPoolDistribution { get; set; } = new();
        public Dictionary<string, decimal> DexVolumeDistribution { get; set; } = new();

        // Network statistics
        public Dictionary<string, decimal> NetworkTVL { get; set; } = new();
        public Dictionary<string, int> NetworkPools { get; set; } = new();

        // Time series data
        public List<decimal> TVLHistory { get; set; } = new();
        public List<decimal> VolumeHistory { get; set; } = new();
        public List<decimal> PriceHistory { get; set; } = new();
        public List<DateTime> HistoryDates { get; set; } = new();

        // Growth metrics
        [Display(Name = "TVL Growth 24h")]
        public decimal TVLGrowth24h { get; set; }

        [Display(Name = "TVL Growth 7d")]
        public decimal TVLGrowth7d { get; set; }

        [Display(Name = "TVL Growth 30d")]
        public decimal TVLGrowth30d { get; set; }

        [Display(Name = "Volume Growth 24h")]
        public decimal VolumeGrowth24h { get; set; }

        [Display(Name = "Volume Growth 7d")]
        public decimal VolumeGrowth7d { get; set; }

        [Display(Name = "User Growth 24h")]
        public decimal UserGrowth24h { get; set; }

        [Display(Name = "User Growth 7d")]
        public decimal UserGrowth7d { get; set; }

        // Display properties
        [Display(Name = "TVL Display")]
        public string TotalValueLockedDisplay => FormatCurrency(TotalValueLocked);

        [Display(Name = "24h Volume Display")]
        public string Volume24hDisplay => FormatCurrency(Volume24h);

        [Display(Name = "7d Volume Display")]
        public string Volume7dDisplay => FormatCurrency(Volume7d);

        [Display(Name = "30d Volume Display")]
        public string Volume30dDisplay => FormatCurrency(Volume30d);

        [Display(Name = "TEACH Liquidity Display")]
        public string TeachTotalLiquidityDisplay => FormatCurrency(TeachTotalLiquidity);

        [Display(Name = "TEACH APY Display")]
        public string TeachAverageAPYDisplay => $"{TeachAverageAPY:F2}%";

        [Display(Name = "TEACH Price Display")]
        public string TeachCurrentPriceDisplay => $"${TeachCurrentPrice:F4}";

        [Display(Name = "TEACH Change Display")]
        public string TeachPriceChange24hDisplay => $"{(TeachPriceChange24h >= 0 ? "+" : "")}{TeachPriceChange24h:F2}%";

        [Display(Name = "TEACH Volume Display")]
        public string TeachVolume24hDisplay => FormatCurrency(TeachVolume24h);

        // Growth displays
        [Display(Name = "TVL Growth 24h Display")]
        public string TVLGrowth24hDisplay => $"{(TVLGrowth24h >= 0 ? "+" : "")}{TVLGrowth24h:F2}%";

        [Display(Name = "TVL Growth 7d Display")]
        public string TVLGrowth7dDisplay => $"{(TVLGrowth7d >= 0 ? "+" : "")}{TVLGrowth7d:F2}%";

        [Display(Name = "TVL Growth 30d Display")]
        public string TVLGrowth30dDisplay => $"{(TVLGrowth30d >= 0 ? "+" : "")}{TVLGrowth30d:F2}%";

        [Display(Name = "Volume Growth 24h Display")]
        public string VolumeGrowth24hDisplay => $"{(VolumeGrowth24h >= 0 ? "+" : "")}{VolumeGrowth24h:F2}%";

        [Display(Name = "Volume Growth 7d Display")]
        public string VolumeGrowth7dDisplay => $"{(VolumeGrowth7d >= 0 ? "+" : "")}{VolumeGrowth7d:F2}%";

        [Display(Name = "User Growth 24h Display")]
        public string UserGrowth24hDisplay => $"{(UserGrowth24h >= 0 ? "+" : "")}{UserGrowth24h:F1}";

        [Display(Name = "User Growth 7d Display")]
        public string UserGrowth7dDisplay => $"{(UserGrowth7d >= 0 ? "+" : "")}{UserGrowth7d:F1}";

        // Status and visual classes
        public string TVLGrowthClass => TVLGrowth24h >= 0 ? "growth-positive" : "growth-negative";
        public string VolumeGrowthClass => VolumeGrowth24h >= 0 ? "growth-positive" : "growth-negative";
        public string UserGrowthClass => UserGrowth24h >= 0 ? "growth-positive" : "growth-negative";
        public string TeachPriceClass => TeachPriceChange24h >= 0 ? "price-up" : "price-down";

        // Market health indicators
        public string MarketHealth => CalculateMarketHealth();
        public string MarketHealthClass => $"health-{MarketHealth.ToLower()}";

        // Pool activity levels
        public decimal AveragePoolAPY { get; set; }
        public decimal MedianPoolAPY { get; set; }
        public decimal PoolAPYStandardDeviation { get; set; }

        public string AveragePoolAPYDisplay => $"{AveragePoolAPY:F2}%";
        public string MedianPoolAPYDisplay => $"{MedianPoolAPY:F2}%";

        // Liquidity concentration
        public decimal Top5PoolsTVLPercentage { get; set; }
        public decimal Top10PoolsTVLPercentage { get; set; }

        public string Top5PoolsTVLDisplay => $"{Top5PoolsTVLPercentage:F1}%";
        public string Top10PoolsTVLDisplay => $"{Top10PoolsTVLPercentage:F1}%";

        // Risk metrics
        public decimal AverageImpermanentLoss { get; set; }
        public decimal HighRiskPoolsPercentage { get; set; }

        public string AverageImpermanentLossDisplay => $"{AverageImpermanentLoss:F2}%";
        public string HighRiskPoolsDisplay => $"{HighRiskPoolsPercentage:F1}%";

        // Trending data
        public List<string> TrendingPools { get; set; } = new();
        public List<string> EmergingOpportunities { get; set; } = new();
        public List<string> RiskAlerts { get; set; } = new();

        // Rankings for UI
        public List<LiquidityPoolRanking> TopPoolsByTVL { get; set; } = new();
        public List<LiquidityPoolRanking> TopPoolsByAPY { get; set; } = new();
        public List<LiquidityPoolRanking> TopPoolsByVolume { get; set; } = new();

        // Helper methods
        private static string FormatCurrency(decimal amount)
        {
            return amount switch
            {
                >= 1_000_000_000 => $"${amount / 1_000_000_000:F2}B",
                >= 1_000_000 => $"${amount / 1_000_000:F2}M",
                >= 1_000 => $"${amount / 1_000:F2}K",
                _ => $"${amount:F2}"
            };
        }

        private string CalculateMarketHealth()
        {
            var score = 0;

            // TVL growth indicators
            if (TVLGrowth24h > 5) score += 2;
            else if (TVLGrowth24h > 0) score += 1;
            else if (TVLGrowth24h < -10) score -= 2;
            else if (TVLGrowth24h < 0) score -= 1;

            // Volume indicators
            if (VolumeGrowth24h > 10) score += 2;
            else if (VolumeGrowth24h > 0) score += 1;
            else if (VolumeGrowth24h < -20) score -= 2;
            else if (VolumeGrowth24h < 0) score -= 1;

            // User growth indicators
            if (UserGrowth24h > 5) score += 1;
            else if (UserGrowth24h < -5) score -= 1;

            // Pool activity
            if (ActivePools > TotalPools * 0.8m) score += 1;
            else if (ActivePools < TotalPools * 0.5m) score -= 1;

            return score switch
            {
                >= 4 => "Excellent",
                >= 2 => "Good",
                >= 0 => "Fair",
                >= -2 => "Poor",
                _ => "Critical"
            };
        }
    }

    /// <summary>
    /// Model for pool rankings display
    /// </summary>
    public class LiquidityPoolRanking
    {
        public int Rank { get; set; }
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public string DexName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string MetricType { get; set; } = string.Empty; // TVL, APY, Volume

        public string ValueDisplay => MetricType switch
        {
            "APY" => $"{Value:F2}%",
            "TVL" or "Volume" => FormatCurrency(Value),
            _ => Value.ToString("F2")
        };

        public string RankDisplay => $"#{Rank}";
        public string RankClass => Rank switch
        {
            1 => "rank-first",
            2 => "rank-second",
            3 => "rank-third",
            _ => "rank-other"
        };

        private static string FormatCurrency(decimal amount)
        {
            return amount switch
            {
                >= 1_000_000 => $"${amount / 1_000_000:F2}M",
                >= 1_000 => $"${amount / 1_000:F2}K",
                _ => $"${amount:F2}"
            };
        }
    }
}