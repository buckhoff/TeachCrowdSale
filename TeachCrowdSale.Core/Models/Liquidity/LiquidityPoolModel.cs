using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Web display model for liquidity pool information
    /// Maps from LiquidityPoolResponse for web layer consumption
    /// </summary>
    public class LiquidityPoolModel
    {
        public int Id { get; set; }

        [Display(Name = "Pool Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Exchange")]
        public string DexName { get; set; } = string.Empty;

        [Display(Name = "Trading Pair")]
        public string TokenPair { get; set; } = string.Empty;

        [EthereumAddress]
        [Display(Name = "Pool Address")]
        public string PoolAddress { get; set; } = string.Empty;

        [EthereumAddress]
        [Display(Name = "Token 0 Address")]
        public string Token0Address { get; set; } = string.Empty;

        [EthereumAddress]
        [Display(Name = "Token 1 Address")]
        public string Token1Address { get; set; } = string.Empty;

        [Display(Name = "Token 0 Symbol")]
        public string Token0Symbol { get; set; } = string.Empty;

        [Display(Name = "Token 1 Symbol")]
        public string Token1Symbol { get; set; } = string.Empty;

        public int Token0Decimals { get; set; } = 18;
        public int Token1Decimals { get; set; } = 18;

        [Display(Name = "Total Value Locked")]
        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Display(Name = "24h Volume")]
        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        [Display(Name = "7d Volume")]
        [Range(0, double.MaxValue)]
        public decimal Volume7d { get; set; }

        [Display(Name = "Fee Percentage")]
        [Range(0, 100)]
        public decimal FeePercentage { get; set; }

        [Display(Name = "APY")]
        [Range(0, 1000)]
        public decimal APY { get; set; }

        [Display(Name = "APR")]
        [Range(0, 1000)]
        public decimal APR { get; set; }

        [Display(Name = "Token 0 Reserve")]
        [Range(0, double.MaxValue)]
        public decimal Token0Reserve { get; set; }

        [Display(Name = "Token 1 Reserve")]
        [Range(0, double.MaxValue)]
        public decimal Token1Reserve { get; set; }

        [Display(Name = "Current Price")]
        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "DEX URL")]
        public string? DexUrl { get; set; }

        [Display(Name = "Analytics URL")]
        public string? AnalyticsUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastSyncAt { get; set; }

        // Computed/Display Properties for Web UI
        [Display(Name = "TVL Display")]
        public string TotalValueLockedDisplay => FormatCurrency(TotalValueLocked);

        [Display(Name = "24h Volume Display")]
        public string Volume24hDisplay => FormatCurrency(Volume24h);

        [Display(Name = "7d Volume Display")]
        public string Volume7dDisplay => FormatCurrency(Volume7d);

        [Display(Name = "APY Display")]
        public string APYDisplay => $"{APY:F2}%";

        [Display(Name = "APR Display")]
        public string APRDisplay => $"{APR:F2}%";

        [Display(Name = "Fee Display")]
        public string FeeDisplay => $"{FeePercentage:F3}%";

        [Display(Name = "Price Display")]
        public string CurrentPriceDisplay => FormatTokenAmount(CurrentPrice);

        [Display(Name = "Token 0 Reserve Display")]
        public string Token0ReserveDisplay => FormatTokenAmount(Token0Reserve);

        [Display(Name = "Token 1 Reserve Display")]
        public string Token1ReserveDisplay => FormatTokenAmount(Token1Reserve);

        // Status indicators for UI
        public string StatusClass => IsActive ? "active" : "inactive";
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string FeaturedClass => IsFeatured ? "featured" : "";

        // Time-based display properties
        public string LastSyncDisplay => FormatTimeAgo(LastSyncAt);
        public string CreatedAtDisplay => CreatedAt.ToString("MMM dd, yyyy");

        // Performance indicators
        public string APYClass => APY >= 50 ? "high-yield" : APY >= 20 ? "medium-yield" : "low-yield";
        public string VolumeClass => Volume24h >= 1000000 ? "high-volume" : Volume24h >= 100000 ? "medium-volume" : "low-volume";

        // Action availability
        public bool CanAddLiquidity => IsActive;
        public bool CanViewAnalytics => !string.IsNullOrEmpty(AnalyticsUrl);
        public bool CanViewOnDex => !string.IsNullOrEmpty(DexUrl);

        // Chart data properties (for UI components)
        public List<decimal> VolumeHistory { get; set; } = new();
        public List<decimal> PriceHistory { get; set; } = new();
        public List<decimal> APYHistory { get; set; } = new();

        // Recommendation score for sorting/filtering
        public decimal RecommendationScore { get; set; }
        public string RecommendationReason { get; set; } = string.Empty;

        // Risk indicators
        public string RiskLevel { get; set; } = "Medium"; // Low, Medium, High
        public string RiskLevelClass => RiskLevel.ToLower();
        public List<string> RiskFactors { get; set; } = new();

        // Helper methods for formatting
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

        private static string FormatTokenAmount(decimal amount)
        {
            return amount switch
            {
                >= 1_000_000_000 => $"{amount / 1_000_000_000:F2}B",
                >= 1_000_000 => $"{amount / 1_000_000:F2}M",
                >= 1_000 => $"{amount / 1_000:F2}K",
                _ => $"{amount:F4}"
            };
        }

        private static string FormatTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            return timeSpan.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1440 => $"{(int)timeSpan.TotalHours}h ago",
                _ => $"{(int)timeSpan.TotalDays}d ago"
            };
        }
    }
}