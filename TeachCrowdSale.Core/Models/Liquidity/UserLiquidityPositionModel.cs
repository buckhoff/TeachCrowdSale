using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Web display model for user liquidity position information
    /// Maps from UserLiquidityPositionResponse for web layer consumption
    /// </summary>
    public class UserLiquidityPositionModel
    {
        public int Id { get; set; }

        public int PoolId { get; set; }

        [Display(Name = "Pool Name")]
        public string PoolName { get; set; } = string.Empty;

        [Display(Name = "Trading Pair")]
        public string TokenPair { get; set; } = string.Empty;

        [Display(Name = "Exchange")]
        public string DexName { get; set; } = string.Empty;

        [EthereumAddress]
        [Display(Name = "Wallet Address")]
        public string WalletAddress { get; set; } = string.Empty;

        [Display(Name = "LP Token Amount")]
        [Range(0, double.MaxValue)]
        public decimal LpTokenAmount { get; set; }

        [Display(Name = "Token 0 Amount")]
        [Range(0, double.MaxValue)]
        public decimal Token0Amount { get; set; }

        [Display(Name = "Token 1 Amount")]
        [Range(0, double.MaxValue)]
        public decimal Token1Amount { get; set; }

        [Display(Name = "Initial Value (USD)")]
        [Range(0, double.MaxValue)]
        public decimal InitialValueUsd { get; set; }

        [Display(Name = "Current Value (USD)")]
        [Range(0, double.MaxValue)]
        public decimal CurrentValueUsd { get; set; }

        [Display(Name = "Fees Earned (USD)")]
        [Range(0, double.MaxValue)]
        public decimal FeesEarnedUsd { get; set; }

        [Display(Name = "Impermanent Loss")]
        public decimal ImpermanentLoss { get; set; }

        [Display(Name = "Net P&L")]
        public decimal NetPnL { get; set; }

        [Display(Name = "P&L Percentage")]
        public decimal PnLPercentage { get; set; }

        [Display(Name = "Token 0 Symbol")]
        public string Token0Symbol { get; set; } = string.Empty;

        [Display(Name = "Token 1 Symbol")]
        public string Token1Symbol { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Display(Name = "Added Date")]
        public DateTime AddedAt { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime LastUpdatedAt { get; set; }

        public string? AddTransactionHash { get; set; }
        public string? RemoveTransactionHash { get; set; }

        // Action Capabilities
        public bool CanRemove { get; set; } = true;
        public bool CanClaimFees { get; set; }
        public bool CanAddMore { get; set; } = true;

        // Display Properties for Web UI
        [Display(Name = "LP Token Display")]
        public string LpTokenAmountDisplay => FormatTokenAmount(LpTokenAmount);

        [Display(Name = "Token 0 Display")]
        public string Token0AmountDisplay => $"{FormatTokenAmount(Token0Amount)} {Token0Symbol}";

        [Display(Name = "Token 1 Display")]
        public string Token1AmountDisplay => $"{FormatTokenAmount(Token1Amount)} {Token1Symbol}";

        [Display(Name = "Initial Value Display")]
        public string InitialValueDisplay => FormatCurrency(InitialValueUsd);

        [Display(Name = "Current Value Display")]
        public string CurrentValueDisplay => FormatCurrency(CurrentValueUsd);

        [Display(Name = "Fees Earned Display")]
        public string FeesEarnedDisplay => FormatCurrency(FeesEarnedUsd);

        [Display(Name = "P&L Display")]
        public string PnLDisplay => FormatCurrency(NetPnL, 2, true);

        [Display(Name = "P&L Percentage Display")]
        public string PnLPercentageDisplay => $"{(PnLPercentage >= 0 ? "+" : "")}{PnLPercentage:F2}%";

        [Display(Name = "Impermanent Loss Display")]
        public string ImpermanentLossDisplay => $"{ImpermanentLoss:F4}%";

        // Status and Visual Classes
        public string StatusClass => IsActive ? "position-active" : "position-inactive";
        public string StatusText => IsActive ? "Active" : "Removed";

        public string PnLClass => NetPnL >= 0 ? "pnl-positive" : "pnl-negative";
        public string PnLIcon => NetPnL >= 0 ? "📈" : "📉";

        public string ImpermanentLossClass => ImpermanentLoss switch
        {
            < -10 => "il-high",
            < -5 => "il-medium",
            < -1 => "il-low",
            _ => "il-minimal"
        };

        // Time-based displays
        public string AddedAtDisplay => AddedAt.ToString("MMM dd, yyyy");
        public string LastUpdatedDisplay => FormatTimeAgo(LastUpdatedAt);
        public string DaysActive => CalculateDaysActive();

        // Performance metrics
        public decimal DailyReturn => CalculateDailyReturn();
        public decimal APY => CalculateAPY();
        public decimal TotalReturn => CurrentValueUsd + FeesEarnedUsd - InitialValueUsd;
        public decimal TotalReturnPercentage => InitialValueUsd > 0 ? (TotalReturn / InitialValueUsd) * 100 : 0;

        public string DailyReturnDisplay => $"{(DailyReturn >= 0 ? "+" : "")}{DailyReturn:F4}%";
        public string APYDisplay => $"{APY:F2}%";
        public string TotalReturnDisplay => FormatCurrency(TotalReturn, 2, true);
        public string TotalReturnPercentageDisplay => $"{(TotalReturnPercentage >= 0 ? "+" : "")}{TotalReturnPercentage:F2}%";

        // Risk indicators
        public string RiskLevel => CalculateRiskLevel();
        public string RiskLevelClass => $"risk-{RiskLevel.ToLower()}";
        public List<string> RiskFactors { get; set; } = new();

        // Portfolio allocation
        public decimal PortfolioPercentage { get; set; }
        public string PortfolioPercentageDisplay => $"{PortfolioPercentage:F1}%";

        // Transaction links
        public string AddTransactionUrl => GetEtherscanUrl(AddTransactionHash);
        public string RemoveTransactionUrl => GetEtherscanUrl(RemoveTransactionHash);

        // Pool information
        public LiquidityPoolModel? Pool { get; set; }

        // Historical data for charts
        public List<decimal> ValueHistory { get; set; } = new();
        public List<decimal> FeesHistory { get; set; } = new();
        public List<DateTime> HistoryDates { get; set; } = new();

        // Recommendations
        public bool ShouldRebalance { get; set; }
        public string RebalanceReason { get; set; } = string.Empty;
        public bool ShouldHarvest { get; set; }
        public string HarvestRecommendation { get; set; } = string.Empty;

        // Helper Methods
        private static string FormatCurrency(decimal amount, int decimals = 2, bool showSign = false)
        {
            var sign = showSign && amount >= 0 ? "+" : "";
            return amount switch
            {
                >= 1_000_000 => $"{sign}${(amount / 1_000_000).ToString($"F{decimals}")}M",
                >= 1_000 => $"{sign}${(amount / 1_000).ToString($"F{decimals}")}K",
                _ => $"{sign}${(amount).ToString($"F{decimals}")}"
            };
        }

        private static string FormatTokenAmount(decimal amount)
        {
            return amount switch
            {
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

        private string CalculateDaysActive()
        {
            var days = (DateTime.UtcNow - AddedAt).Days;
            return days == 0 ? "Today" : days == 1 ? "1 day" : $"{days} days";
        }

        private decimal CalculateDailyReturn()
        {
            var days = Math.Max(1, (DateTime.UtcNow - AddedAt).Days);
            return days > 0 ? (PnLPercentage / days) : 0;
        }

        private decimal CalculateAPY()
        {
            var days = Math.Max(1, (DateTime.UtcNow - AddedAt).Days);
            var dailyReturn = PnLPercentage / 100 / days;
            return (decimal)(Math.Pow((double)(1 + dailyReturn), 365) - 1) * 100;
        }

        private string CalculateRiskLevel()
        {
            var score = 0;

            // Factor in impermanent loss
            if (ImpermanentLoss < -10) score += 3;
            else if (ImpermanentLoss < -5) score += 2;
            else if (ImpermanentLoss < -1) score += 1;

            // Factor in volatility (could be enhanced with actual volatility data)
            if (Math.Abs(PnLPercentage) > 50) score += 2;
            else if (Math.Abs(PnLPercentage) > 20) score += 1;

            return score switch
            {
                >= 4 => "High",
                >= 2 => "Medium",
                _ => "Low"
            };
        }

        private static string GetEtherscanUrl(string? txHash)
        {
            if (string.IsNullOrEmpty(txHash)) return string.Empty;
            return $"https://etherscan.io/tx/{txHash}";
        }
    }
}