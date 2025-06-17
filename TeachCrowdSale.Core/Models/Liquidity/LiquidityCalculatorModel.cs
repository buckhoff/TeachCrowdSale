using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Web model for liquidity calculation and returns estimation
    /// Provides comprehensive calculations for liquidity provision decisions
    /// </summary>
    public class LiquidityCalculatorModel
    {
        // Input parameters
        public int PoolId { get; set; }

        [Display(Name = "Trading Pair")]
        public string TokenPair { get; set; } = string.Empty;

        [Display(Name = "Token 0 Symbol")]
        public string Token0Symbol { get; set; } = string.Empty;

        [Display(Name = "Token 1 Symbol")]
        public string Token1Symbol { get; set; } = string.Empty;

        [Display(Name = "Token 0 Amount")]
        [Range(0, double.MaxValue)]
        public decimal Token0Amount { get; set; }

        [Display(Name = "Token 1 Amount")]
        [Range(0, double.MaxValue)]
        public decimal Token1Amount { get; set; }

        [Display(Name = "Slippage Tolerance")]
        [Range(0, 100)]
        public decimal SlippageTolerance { get; set; } = 0.5m;

        // Current pool state
        [Display(Name = "Current Pool TVL")]
        public decimal CurrentPoolTVL { get; set; }

        [Display(Name = "Current APY")]
        public decimal CurrentAPY { get; set; }

        [Display(Name = "Current Price")]
        public decimal CurrentPrice { get; set; }

        [Display(Name = "Token 0 Reserve")]
        public decimal Token0Reserve { get; set; }

        [Display(Name = "Token 1 Reserve")]
        public decimal Token1Reserve { get; set; }

        [Display(Name = "Fee Percentage")]
        public decimal FeePercentage { get; set; }

        // Calculated outputs
        [Display(Name = "Expected LP Tokens")]
        public decimal ExpectedLpTokens { get; set; }

        [Display(Name = "Pool Share")]
        public decimal PoolSharePercentage { get; set; }

        [Display(Name = "Total Investment Value")]
        public decimal TotalInvestmentValue { get; set; }

        [Display(Name = "Price Impact")]
        public decimal PriceImpact { get; set; }

        [Display(Name = "Minimum Token 0 Amount")]
        public decimal MinToken0Amount { get; set; }

        [Display(Name = "Minimum Token 1 Amount")]
        public decimal MinToken1Amount { get; set; }

        // Returns estimation
        [Display(Name = "Daily Fees Estimate")]
        public decimal DailyFeesEstimate { get; set; }

        [Display(Name = "Weekly Fees Estimate")]
        public decimal WeeklyFeesEstimate { get; set; }

        [Display(Name = "Monthly Fees Estimate")]
        public decimal MonthlyFeesEstimate { get; set; }

        [Display(Name = "Yearly Fees Estimate")]
        public decimal YearlyFeesEstimate { get; set; }

        // Risk calculations
        [Display(Name = "Impermanent Loss 10%")]
        public decimal ImpermanentLoss10Percent { get; set; }

        [Display(Name = "Impermanent Loss 25%")]
        public decimal ImpermanentLoss25Percent { get; set; }

        [Display(Name = "Impermanent Loss 50%")]
        public decimal ImpermanentLoss50Percent { get; set; }

        [Display(Name = "Impermanent Loss 100%")]
        public decimal ImpermanentLoss100Percent { get; set; }

        // Breakeven analysis
        [Display(Name = "Days to Breakeven 10%")]
        public int DaysToBreakeven10Percent { get; set; }

        [Display(Name = "Days to Breakeven 25%")]
        public int DaysToBreakeven25Percent { get; set; }

        [Display(Name = "Days to Breakeven 50%")]
        public int DaysToBreakeven50Percent { get; set; }

        // Gas and fees
        [Display(Name = "Estimated Gas Cost")]
        public decimal EstimatedGasCost { get; set; }

        [Display(Name = "Approval Gas Cost")]
        public decimal ApprovalGasCost { get; set; }

        [Display(Name = "Add Liquidity Gas Cost")]
        public decimal AddLiquidityGasCost { get; set; }

        [Display(Name = "Total Transaction Cost")]
        public decimal TotalTransactionCost { get; set; }

        // Display properties
        [Display(Name = "Token 0 Amount Display")]
        public string Token0AmountDisplay => $"{Token0Amount:F4} {Token0Symbol}";

        [Display(Name = "Token 1 Amount Display")]
        public string Token1AmountDisplay => $"{Token1Amount:F4} {Token1Symbol}";

        [Display(Name = "LP Tokens Display")]
        public string ExpectedLpTokensDisplay => $"{ExpectedLpTokens:F6} LP";

        [Display(Name = "Pool Share Display")]
        public string PoolShareDisplay => $"{PoolSharePercentage:F4}%";

        [Display(Name = "Investment Value Display")]
        public string TotalInvestmentValueDisplay => $"${TotalInvestmentValue:F2}";

        [Display(Name = "Price Impact Display")]
        public string PriceImpactDisplay => $"{PriceImpact:F3}%";

        [Display(Name = "APY Display")]
        public string CurrentAPYDisplay => $"{CurrentAPY:F2}%";

        [Display(Name = "Current Price Display")]
        public string CurrentPriceDisplay => $"{CurrentPrice:F6}";

        // Fee estimates display
        [Display(Name = "Daily Fees Display")]
        public string DailyFeesDisplay => $"${DailyFeesEstimate:F2}";

        [Display(Name = "Weekly Fees Display")]
        public string WeeklyFeesDisplay => $"${WeeklyFeesEstimate:F2}";

        [Display(Name = "Monthly Fees Display")]
        public string MonthlyFeesDisplay => $"${MonthlyFeesEstimate:F2}";

        [Display(Name = "Yearly Fees Display")]
        public string YearlyFeesDisplay => $"${YearlyFeesEstimate:F2}";

        // Impermanent loss display
        [Display(Name = "IL 10% Display")]
        public string ImpermanentLoss10Display => $"{ImpermanentLoss10Percent:F2}%";

        [Display(Name = "IL 25% Display")]
        public string ImpermanentLoss25Display => $"{ImpermanentLoss25Percent:F2}%";

        [Display(Name = "IL 50% Display")]
        public string ImpermanentLoss50Display => $"{ImpermanentLoss50Percent:F2}%";

        [Display(Name = "IL 100% Display")]
        public string ImpermanentLoss100Display => $"{ImpermanentLoss100Percent:F2}%";

        // Breakeven display
        [Display(Name = "Breakeven 10% Display")]
        public string Breakeven10Display => FormatDays(DaysToBreakeven10Percent);

        [Display(Name = "Breakeven 25% Display")]
        public string Breakeven25Display => FormatDays(DaysToBreakeven25Percent);

        [Display(Name = "Breakeven 50% Display")]
        public string Breakeven50Display => FormatDays(DaysToBreakeven50Percent);

        // Gas costs display
        [Display(Name = "Gas Cost Display")]
        public string EstimatedGasCostDisplay => $"${EstimatedGasCost:F2}";

        [Display(Name = "Total Cost Display")]
        public string TotalTransactionCostDisplay => $"${TotalTransactionCost:F2}";

        // Visual indicators
        public string PriceImpactClass => PriceImpact switch
        {
            < 0.1m => "impact-minimal",
            < 1.0m => "impact-low",
            < 3.0m => "impact-medium",
            < 5.0m => "impact-high",
            _ => "impact-critical"
        };

        public string APYClass => CurrentAPY switch
        {
            < 5 => "apy-low",
            < 20 => "apy-medium",
            < 50 => "apy-high",
            _ => "apy-exceptional"
        };

        // Risk level assessment
        public string RiskLevel => CalculateRiskLevel();
        public string RiskLevelClass => $"risk-{RiskLevel.ToLower()}";

        // Recommendation
        public string Recommendation => GenerateRecommendation();
        public string RecommendationClass => Recommendation.Contains("Recommended") ? "rec-positive" :
                                           Recommendation.Contains("Caution") ? "rec-warning" : "rec-negative";

        // Scenario analysis
        public List<ScenarioAnalysis> Scenarios { get; set; } = new();

        // Comparison with other pools
        public bool IsBetterThanAverage { get; set; }
        public decimal AverageMarketAPY { get; set; }
        public string ComparisonText => IsBetterThanAverage ?
            $"This pool offers {CurrentAPY - AverageMarketAPY:F1}% higher APY than market average" :
            $"This pool offers {AverageMarketAPY - CurrentAPY:F1}% lower APY than market average";

        // Advanced metrics
        public decimal Sharpe Ratio { get; set; }
        public decimal MaxDrawdown { get; set; }
        public decimal Volatility { get; set; }

        public string SharpeRatioDisplay => $"{SharpeRatio:F2}";
        public string MaxDrawdownDisplay => $"{MaxDrawdown:F2}%";
        public string VolatilityDisplay => $"{Volatility:F2}%";

        // Time-based projections
        public Dictionary<int, decimal> ProjectedValues { get; set; } = new(); // Days -> Value
        public Dictionary<int, decimal> ProjectedFees { get; set; } = new(); // Days -> Fees

        // Helper methods
        private static string FormatDays(int days)
        {
            if (days <= 0) return "N/A";
            if (days == 1) return "1 day";
            if (days < 30) return $"{days} days";
            if (days < 365) return $"{days / 30:F1} months";
            return $"{days / 365:F1} years";
        }

        private string CalculateRiskLevel()
        {
            var score = 0;

            // Price impact assessment
            if (PriceImpact > 5) score += 3;
            else if (PriceImpact > 2) score += 2;
            else if (PriceImpact > 1) score += 1;

            // APY assessment (very high APY can indicate higher risk)
            if (CurrentAPY > 100) score += 2;
            else if (CurrentAPY > 50) score += 1;

            // Pool size assessment (smaller pools are riskier)
            if (CurrentPoolTVL < 100000) score += 2;
            else if (CurrentPoolTVL < 1000000) score += 1;

            // IL assessment
            if (ImpermanentLoss25Percent > 10) score += 2;
            else if (ImpermanentLoss25Percent > 5) score += 1;

            return score switch
            {
                >= 6 => "High",
                >= 3 => "Medium",
                _ => "Low"
            };
        }

        private string GenerateRecommendation()
        {
            var issues = new List<string>();

            if (PriceImpact > 5)
                issues.Add("very high price impact");

            if (CurrentAPY < 5)
                issues.Add("low returns");

            if (CurrentPoolTVL < 100000)
                issues.Add("low liquidity");

            if (ImpermanentLoss25Percent > 15)
                issues.Add("high impermanent loss risk");

            if (DaysToBreakeven25Percent > 365)
                issues.Add("long breakeven period");

            if (!issues.Any())
            {
                return "✅ Recommended - Good risk/reward ratio";
            }
            else if (issues.Count <= 2)
            {
                return $"⚠️ Proceed with Caution - {string.Join(", ", issues)}";
            }
            else
            {
                return $"❌ Not Recommended - Multiple risk factors: {string.Join(", ", issues)}";
            }
        }

        // Calculate optimal amounts based on current reserves
        public (decimal optimalToken0, decimal optimalToken1) CalculateOptimalAmounts(decimal inputAmount, bool isToken0Input)
        {
            if (Token0Reserve <= 0 || Token1Reserve <= 0)
                return (0, 0);

            if (isToken0Input)
            {
                var ratio = Token1Reserve / Token0Reserve;
                return (inputAmount, inputAmount * ratio);
            }
            else
            {
                var ratio = Token0Reserve / Token1Reserve;
                return (inputAmount * ratio, inputAmount);
            }
        }

        // Update all calculations
        public void RecalculateAll()
        {
            CalculatePoolShare();
            CalculatePriceImpact();
            CalculateFeesEstimate();
            CalculateImpermanentLoss();
            CalculateBreakevenPeriods();
            CalculateGasCosts();
            GenerateScenarios();
        }

        private void CalculatePoolShare()
        {
            var newPoolTVL = CurrentPoolTVL + TotalInvestmentValue;
            PoolSharePercentage = newPoolTVL > 0 ? (TotalInvestmentValue / newPoolTVL) * 100 : 0;
        }

        private void CalculatePriceImpact()
        {
            // Simplified price impact calculation
            var poolDepth = Math.Min(Token0Reserve, Token1Reserve);
            var tradeSize = Math.Max(Token0Amount, Token1Amount);
            PriceImpact = poolDepth > 0 ? (tradeSize / poolDepth) * 100 : 0;
        }

        private void CalculateFeesEstimate()
        {
            var dailyVolume = CurrentPoolTVL * 0.1m; // Assume 10% daily turnover
            var dailyFees = dailyVolume * (FeePercentage / 100);
            var userShare = PoolSharePercentage / 100;

            DailyFeesEstimate = dailyFees * userShare;
            WeeklyFeesEstimate = DailyFeesEstimate * 7;
            MonthlyFeesEstimate = DailyFeesEstimate * 30;
            YearlyFeesEstimate = DailyFeesEstimate * 365;
        }

        private void CalculateImpermanentLoss()
        {
            // Simplified IL calculation for different price changes
            ImpermanentLoss10Percent = CalculateILForPriceChange(1.1m);
            ImpermanentLoss25Percent = CalculateILForPriceChange(1.25m);
            ImpermanentLoss50Percent = CalculateILForPriceChange(1.5m);
            ImpermanentLoss100Percent = CalculateILForPriceChange(2.0m);
        }

        private decimal CalculateILForPriceChange(decimal priceMultiplier)
        {
            // IL = 2 * sqrt(price_ratio) / (1 + price_ratio) - 1
            var sqrt = (decimal)Math.Sqrt((double)priceMultiplier);
            var il = (2 * sqrt) / (1 + priceMultiplier) - 1;
            return Math.Abs(il) * 100; // Convert to percentage
        }

        private void CalculateBreakevenPeriods()
        {
            if (DailyFeesEstimate <= 0) return;

            DaysToBreakeven10Percent = ImpermanentLoss10Percent > 0 ?
                (int)Math.Ceiling((ImpermanentLoss10Percent / 100 * TotalInvestmentValue) / DailyFeesEstimate) : 0;

            DaysToBreakeven25Percent = ImpermanentLoss25Percent > 0 ?
                (int)Math.Ceiling((ImpermanentLoss25Percent / 100 * TotalInvestmentValue) / DailyFeesEstimate) : 0;

            Days