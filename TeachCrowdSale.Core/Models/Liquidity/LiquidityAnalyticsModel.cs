using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Web display model for advanced liquidity analytics and insights
    /// Maps from LiquidityAnalyticsResponse for web layer consumption
    /// </summary>
    public class LiquidityAnalyticsModel
    {
        // Time range for analytics
        [Display(Name = "Analysis Period")]
        public string AnalysisPeriod { get; set; } = "30d";

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        // Market overview metrics
        [Display(Name = "Total Market TVL")]
        public decimal TotalMarketTVL { get; set; }

        [Display(Name = "Market TVL Change")]
        public decimal MarketTVLChange { get; set; }

        [Display(Name = "Total Market Volume")]
        public decimal TotalMarketVolume { get; set; }

        [Display(Name = "Market Volume Change")]
        public decimal MarketVolumeChange { get; set; }

        [Display(Name = "Average Market APY")]
        public decimal AverageMarketAPY { get; set; }

        [Display(Name = "Market APY Change")]
        public decimal MarketAPYChange { get; set; }

        // TEACH token specific analytics
        [Display(Name = "TEACH Total TVL")]
        public decimal TeachTotalTVL { get; set; }

        [Display(Name = "TEACH TVL Market Share")]
        public decimal TeachTVLMarketShare { get; set; }

        [Display(Name = "TEACH Volume")]
        public decimal TeachVolume { get; set; }

        [Display(Name = "TEACH Volume Share")]
        public decimal TeachVolumeShare { get; set; }

        [Display(Name = "TEACH Price")]
        public decimal TeachPrice { get; set; }

        [Display(Name = "TEACH Price Change")]
        public decimal TeachPriceChange { get; set; }

        [Display(Name = "TEACH Volatility")]
        public decimal TeachVolatility { get; set; }

        // Pool performance analytics
        [Display(Name = "Top Performing Pools")]
        public List<PoolPerformanceModel> TopPerformingPools { get; set; } = new();

        [Display(Name = "Underperforming Pools")]
        public List<PoolPerformanceModel> UnderperformingPools { get; set; } = new();

        [Display(Name = "New Pools")]
        public List<PoolPerformanceModel> NewPools { get; set; } = new();

        [Display(Name = "Trending Pools")]
        public List<PoolPerformanceModel> TrendingPools { get; set; } = new();

        // DEX analytics
        [Display(Name = "DEX Performance")]
        public List<DexPerformanceModel> DexPerformance { get; set; } = new();

        [Display(Name = "DEX Market Share")]
        public Dictionary<string, decimal> DexMarketShare { get; set; } = new();

        [Display(Name = "DEX Growth Rates")]
        public Dictionary<string, decimal> DexGrowthRates { get; set; } = new();

        // User behavior analytics
        [Display(Name = "Total Unique Users")]
        public int TotalUniqueUsers { get; set; }

        [Display(Name = "New Users")]
        public int NewUsers { get; set; }

        [Display(Name = "Active Users")]
        public int ActiveUsers { get; set; }

        [Display(Name = "User Retention Rate")]
        public decimal UserRetentionRate { get; set; }

        [Display(Name = "Average Position Size")]
        public decimal AveragePositionSize { get; set; }

        [Display(Name = "Average Hold Duration")]
        public decimal AverageHoldDuration { get; set; }

        // Risk analytics
        [Display(Name = "Market Risk Score")]
        public decimal MarketRiskScore { get; set; }

        [Display(Name = "Average Impermanent Loss")]
        public decimal AverageImpermanentLoss { get; set; }

        [Display(Name = "High Risk Pools Count")]
        public int HighRiskPoolsCount { get; set; }

        [Display(Name = "Risk Distribution")]
        public Dictionary<string, int> RiskDistribution { get; set; } = new();

        // Correlation analysis
        [Display(Name = "Price Correlations")]
        public Dictionary<string, decimal> PriceCorrelations { get; set; } = new();

        [Display(Name = "Volume Correlations")]
        public Dictionary<string, decimal> VolumeCorrelations { get; set; } = new();

        // Time series data for charts
        [Display(Name = "TVL History")]
        public List<TimeSeriesDataPoint> TVLHistory { get; set; } = new();

        [Display(Name = "Volume History")]
        public List<TimeSeriesDataPoint> VolumeHistory { get; set; } = new();

        [Display(Name = "APY History")]
        public List<TimeSeriesDataPoint> APYHistory { get; set; } = new();

        [Display(Name = "Price History")]
        public List<TimeSeriesDataPoint> PriceHistory { get; set; } = new();

        // Yield farming opportunities
        [Display(Name = "High Yield Opportunities")]
        public List<YieldOpportunityModel> HighYieldOpportunities { get; set; } = new();

        [Display(Name = "Stable Yield Opportunities")]
        public List<YieldOpportunityModel> StableYieldOpportunities { get; set; } = new();

        [Display(Name = "Low Risk Opportunities")]
        public List<YieldOpportunityModel> LowRiskOpportunities { get; set; } = new();

        // Market predictions and insights
        [Display(Name = "Market Predictions")]
        public List<MarketPredictionModel> MarketPredictions { get; set; } = new();

        [Display(Name = "Insights")]
        public List<string> Insights { get; set; } = new();

        [Display(Name = "Recommendations")]
        public List<string> Recommendations { get; set; } = new();

        [Display(Name = "Risk Alerts")]
        public List<string> RiskAlerts { get; set; } = new();

        // Display properties
        [Display(Name = "Market TVL Display")]
        public string TotalMarketTVLDisplay => FormatCurrency(TotalMarketTVL);

        [Display(Name = "Market TVL Change Display")]
        public string MarketTVLChangeDisplay => FormatPercentage(MarketTVLChange, true);

        [Display(Name = "Market Volume Display")]
        public string TotalMarketVolumeDisplay => FormatCurrency(TotalMarketVolume);

        [Display(Name = "Market Volume Change Display")]
        public string MarketVolumeChangeDisplay => FormatPercentage(MarketVolumeChange, true);

        [Display(Name = "Market APY Display")]
        public string AverageMarketAPYDisplay => FormatPercentage(AverageMarketAPY);

        [Display(Name = "TEACH TVL Display")]
        public string TeachTotalTVLDisplay => FormatCurrency(TeachTotalTVL);

        [Display(Name = "TEACH Market Share Display")]
        public string TeachTVLMarketShareDisplay => FormatPercentage(TeachTVLMarketShare);

        [Display(Name = "TEACH Volume Display")]
        public string TeachVolumeDisplay => FormatCurrency(TeachVolume);

        [Display(Name = "TEACH Price Display")]
        public string TeachPriceDisplay => $"${TeachPrice:F4}";

        [Display(Name = "TEACH Price Change Display")]
        public string TeachPriceChangeDisplay => FormatPercentage(TeachPriceChange, true);

        [Display(Name = "TEACH Volatility Display")]
        public string TeachVolatilityDisplay => FormatPercentage(TeachVolatility);

        [Display(Name = "User Retention Display")]
        public string UserRetentionRateDisplay => FormatPercentage(UserRetentionRate);

        [Display(Name = "Average Position Display")]
        public string AveragePositionSizeDisplay => FormatCurrency(AveragePositionSize);

        [Display(Name = "Average Duration Display")]
        public string AverageHoldDurationDisplay => FormatDays(AverageHoldDuration);

        [Display(Name = "Risk Score Display")]
        public string MarketRiskScoreDisplay => $"{MarketRiskScore:F1}/10";

        [Display(Name = "Average IL Display")]
        public string AverageImpermanentLossDisplay => FormatPercentage(AverageImpermanentLoss);

        // Status and visual classes
        public string MarketTVLChangeClass => MarketTVLChange >= 0 ? "change-positive" : "change-negative";
        public string MarketVolumeChangeClass => MarketVolumeChange >= 0 ? "change-positive" : "change-negative";
        public string TeachPriceChangeClass => TeachPriceChange >= 0 ? "change-positive" : "change-negative";

        public string MarketRiskClass => MarketRiskScore switch
        {
            < 3 => "risk-low",
            < 6 => "risk-medium",
            < 8 => "risk-high",
            _ => "risk-critical"
        };

        public string TeachVolatilityClass => TeachVolatility switch
        {
            < 10 => "volatility-low",
            < 25 => "volatility-medium",
            < 50 => "volatility-high",
            _ => "volatility-extreme"
        };

        // Summary metrics
        public string MarketHealthSummary => GenerateMarketHealthSummary();
        public string TeachPerformanceSummary => GenerateTeachPerformanceSummary();
        public string RiskAssessmentSummary => GenerateRiskAssessmentSummary();

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

        private static string FormatPercentage(decimal percentage, bool showSign = false)
        {
            var sign = showSign && percentage >= 0 ? "+" : "";
            return $"{sign}{percentage:F2}%";
        }

        private static string FormatDays(decimal days)
        {
            if (days < 1) return $"{days * 24:F1} hours";
            if (days < 30) return $"{days:F1} days";
            if (days < 365) return $"{days / 30:F1} months";
            return $"{days / 365:F1} years";
        }

        private string GenerateMarketHealthSummary()
        {
            var positive = 0;
            var negative = 0;

            if (MarketTVLChange > 0) positive++; else negative++;
            if (MarketVolumeChange > 0) positive++; else negative++;
            if (MarketAPYChange > 0) positive++; else negative++;
            if (UserRetentionRate > 70) positive++; else negative++;
            if (MarketRiskScore < 5) positive++; else negative++;

            return positive > negative ? "Market conditions are favorable with positive growth trends" :
                   positive == negative ? "Market conditions are mixed with both positive and negative indicators" :
                   "Market conditions show concerning trends requiring careful monitoring";
        }

        private string GenerateTeachPerformanceSummary()
        {
            var performance = "TEACH token ";

            if (TeachPriceChange > 10)
                performance += "is showing strong performance with significant price gains";
            else if (TeachPriceChange > 0)
                performance += "is performing well with modest price increases";
            else if (TeachPriceChange > -10)
                performance += "is experiencing minor volatility with small price declines";
            else
                performance += "is facing significant price pressure";

            if (TeachTVLMarketShare > 5)
                performance += " and maintains a strong market presence";
            else if (TeachTVLMarketShare > 1)
                performance += " with moderate market share";
            else
                performance += " but has limited market penetration";

            return performance + ".";
        }

        private string GenerateRiskAssessmentSummary()
        {
            if (MarketRiskScore < 3)
                return "Overall market risk is low with stable conditions across most pools";
            else if (MarketRiskScore < 6)
                return "Market risk is moderate requiring standard risk management practices";
            else if (MarketRiskScore < 8)
                return "Market risk is elevated with increased volatility and potential losses";
            else
                return "Market risk is high with significant potential for losses across multiple pools";
        }
    }
}
