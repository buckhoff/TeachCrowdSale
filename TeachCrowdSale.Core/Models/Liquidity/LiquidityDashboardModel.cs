using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Main dashboard model for liquidity section
    /// Aggregates data from multiple sources for comprehensive dashboard view
    /// Maps from LiquidityPageDataResponse
    /// </summary>
    public class LiquidityDashboardModel
    {
        // User-specific data
        [EthereumAddress]
        [Display(Name = "Wallet Address")]
        public string? WalletAddress { get; set; }

        public bool IsWalletConnected => !string.IsNullOrEmpty(WalletAddress);

        // User's liquidity positions
        [Display(Name = "My Positions")]
        public List<UserLiquidityPositionModel> UserPositions { get; set; } = new();

        // Available liquidity pools
        [Display(Name = "Available Pools")]
        public List<LiquidityPoolModel> AvailablePools { get; set; } = new();

        // Featured/recommended pools
        [Display(Name = "Featured Pools")]
        public List<LiquidityPoolModel> FeaturedPools { get; set; } = new();

        // DEX configurations
        [Display(Name = "Supported Exchanges")]
        public List<DexConfigurationModel> SupportedDexes { get; set; } = new();

        // Overall statistics
        [Display(Name = "Total Portfolio Value")]
        public decimal TotalPortfolioValue { get; set; }

        [Display(Name = "Total Fees Earned")]
        public decimal TotalFeesEarned { get; set; }

        [Display(Name = "Total P&L")]
        public decimal TotalPnL { get; set; }

        [Display(Name = "Average APY")]
        public decimal AverageAPY { get; set; }

        [Display(Name = "Active Positions Count")]
        public int ActivePositionsCount { get; set; }

        [Display(Name = "Total Impermanent Loss")]
        public decimal TotalImpermanentLoss { get; set; }

        // Market data
        [Display(Name = "Market Total TVL")]
        public decimal MarketTotalTVL { get; set; }

        [Display(Name = "Market 24h Volume")]
        public decimal Market24hVolume { get; set; }

        [Display(Name = "TEACH Token Price")]
        public decimal TeachTokenPrice { get; set; }

        [Display(Name = "TEACH Price Change 24h")]
        public decimal TeachPriceChange24h { get; set; }

        // Display properties
        [Display(Name = "Total Portfolio Display")]
        public string TotalPortfolioValueDisplay => FormatCurrency(TotalPortfolioValue);

        [Display(Name = "Total Fees Display")]
        public string TotalFeesEarnedDisplay => FormatCurrency(TotalFeesEarned);

        [Display(Name = "Total P&L Display")]
        public string TotalPnLDisplay => FormatCurrency(TotalPnL, 2, true);

        [Display(Name = "Average APY Display")]
        public string AverageAPYDisplay => $"{AverageAPY:F2}%";

        [Display(Name = "Total IL Display")]
        public string TotalImpermanentLossDisplay => $"{TotalImpermanentLoss:F2}%";

        [Display(Name = "Market TVL Display")]
        public string MarketTotalTVLDisplay => FormatCurrency(MarketTotalTVL);

        [Display(Name = "Market Volume Display")]
        public string Market24hVolumeDisplay => FormatCurrency(Market24hVolume);

        [Display(Name = "TEACH Price Display")]
        public string TeachTokenPriceDisplay => $"${TeachTokenPrice:F4}";

        [Display(Name = "TEACH Change Display")]
        public string TeachPriceChange24hDisplay => $"{(TeachPriceChange24h >= 0 ? "+" : "")}{TeachPriceChange24h:F2}%";

        // Status indicators
        public string TotalPnLClass => TotalPnL >= 0 ? "pnl-positive" : "pnl-negative";
        public string TeachPriceChangeClass => TeachPriceChange24h >= 0 ? "price-up" : "price-down";

        // Portfolio breakdown
        public Dictionary<string, decimal> PoolAllocation { get; set; } = new();
        public Dictionary<string, decimal> DexAllocation { get; set; } = new();
        public Dictionary<string, decimal> TokenAllocation { get; set; } = new();

        // Performance metrics
        public decimal PortfolioAPY { get; set; }
        public decimal DailyReturn { get; set; }
        public decimal WeeklyReturn { get; set; }
        public decimal MonthlyReturn { get; set; }

        public string PortfolioAPYDisplay => $"{PortfolioAPY:F2}%";
        public string DailyReturnDisplay => $"{(DailyReturn >= 0 ? "+" : "")}{DailyReturn:F2}%";
        public string WeeklyReturnDisplay => $"{(WeeklyReturn >= 0 ? "+" : "")}{WeeklyReturn:F2}%";
        public string MonthlyReturnDisplay => $"{(MonthlyReturn >= 0 ? "+" : "")}{MonthlyReturn:F2}%";

        // Risk assessment
        public string OverallRiskLevel { get; set; } = "Medium";
        public string OverallRiskLevelClass => $"risk-{OverallRiskLevel.ToLower()}";
        public List<string> RiskFactors { get; set; } = new();
        public List<string> RiskMitigations { get; set; } = new();

        // Opportunities and recommendations
        public List<LiquidityOpportunityModel> Opportunities { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public bool HasRebalanceRecommendation { get; set; }
        public bool HasHarvestRecommendation { get; set; }

        // Chart data
        public List<decimal> PortfolioValueHistory { get; set; } = new();
        public List<decimal> FeesEarnedHistory { get; set; } = new();
        public List<DateTime> HistoryDates { get; set; } = new();

        // Quick actions availability
        public bool CanAddLiquidity => IsWalletConnected;
        public bool CanManagePositions => IsWalletConnected && ActivePositionsCount > 0;
        public bool CanHarvestFees => HasHarvestRecommendation;
        public bool CanRebalance => HasRebalanceRecommendation;

        // Educational content flags
        public bool ShowBeginnersGuide => ActivePositionsCount == 0;
        public bool ShowAdvancedFeatures => ActivePositionsCount > 3;
        public bool ShowRiskWarnings => OverallRiskLevel == "High";

        // Pagination for pools
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalPoolsCount { get; set; }
        public int PageSize { get; set; } = 10;

        // Filtering and sorting
        public string SortBy { get; set; } = "TVL"; // TVL, APY, Volume, etc.
        public string SortDirection { get; set; } = "DESC";
        public string FilterByDex { get; set; } = string.Empty;
        public string FilterByNetwork { get; set; } = string.Empty;
        public decimal MinTVL { get; set; }
        public decimal MinAPY { get; set; }

        // Helper methods
        private static string FormatCurrency(decimal amount, int decimals = 2, bool showSign = false)
        {
            var sign = showSign && amount >= 0 ? "+" : "";
            return amount switch
            {
                >= 1_000_000_000 => $"{sign}${amount / 1_000_000_000:F{decimals}}B",
                >= 1_000_000 => $"{sign}${amount / 1_000_000:F{decimals}}M",
                >= 1_000 => $"{sign}${amount / 1_000:F{decimals}}K",
                _ => $"{sign}${amount:F{decimals}}"
            };
        }

        // Calculate derived properties
        public void CalculateDerivedMetrics()
        {
            // Calculate totals from user positions
            TotalPortfolioValue = UserPositions.Sum(p => p.CurrentValueUsd);
            TotalFeesEarned = UserPositions.Sum(p => p.FeesEarnedUsd);
            TotalPnL = UserPositions.Sum(p => p.NetPnL);
            ActivePositionsCount = UserPositions.Count(p => p.IsActive);

            // Calculate average APY weighted by position value
            if (UserPositions.Any(p => p.IsActive))
            {
                var totalValue = UserPositions.Where(p => p.IsActive).Sum(p => p.CurrentValueUsd);
                if (totalValue > 0)
                {
                    AverageAPY = UserPositions
                        .Where(p => p.IsActive)
                        .Sum(p => p.APY * (p.CurrentValueUsd / totalValue));
                }
            }

            // Calculate allocations
            CalculateAllocations();
        }

        private void CalculateAllocations()
        {
            if (!UserPositions.Any()) return;

            var total = TotalPortfolioValue;
            if (total <= 0) return;

            // Pool allocation
            PoolAllocation = UserPositions
                .Where(p => p.IsActive)
                .GroupBy(p => p.PoolName)
                .ToDictionary(
                    g => g.Key,
                    g => (g.Sum(p => p.CurrentValueUsd) / total) * 100
                );

            // DEX allocation
            DexAllocation = UserPositions
                .Where(p => p.IsActive)
                .GroupBy(p => p.DexName)
                .ToDictionary(
                    g => g.Key,
                    g => (g.Sum(p => p.CurrentValueUsd) / total) * 100
                );

            // Token allocation (simplified - would need more detailed token analysis)
            TokenAllocation = UserPositions
                .Where(p => p.IsActive)
                .SelectMany(p => new[]
                {
                    new { Token = p.Token0Symbol, Value = p.Token0Amount * (p.CurrentValueUsd / 2) / p.Token0Amount },
                    new { Token = p.Token1Symbol, Value = p.Token1Amount * (p.CurrentValueUsd / 2) / p.Token1Amount }
                })
                .GroupBy(t => t.Token)
                .ToDictionary(
                    g => g.Key,
                    g => (g.Sum(t => t.Value) / total) * 100
                );
        }
    }

    /// <summary>
    /// Model for liquidity opportunities and recommendations
    /// </summary>
    public class LiquidityOpportunityModel
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public string DexName { get; set; } = string.Empty;
        public decimal APY { get; set; }
        public decimal TVL { get; set; }
        public decimal RecommendationScore { get; set; }
        public string RecommendationReason { get; set; } = string.Empty;
        public string OpportunityType { get; set; } = string.Empty; // "High Yield", "Low Risk", "New Pool", etc.
        public List<string> Benefits { get; set; } = new();
        public List<string> Risks { get; set; } = new();

        // Display properties
        public string APYDisplay => $"{APY:F2}%";
        public string TVLDisplay => FormatCurrency(TVL);
        public string ScoreDisplay => $"{RecommendationScore:F1}/10";
        public string OpportunityClass => OpportunityType.ToLower().Replace(" ", "-");

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