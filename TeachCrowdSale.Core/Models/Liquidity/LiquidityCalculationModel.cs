using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Liquidity calculation and preview model
    /// </summary>
    public class LiquidityCalculationModel
    {
        public int PoolId { get; set; }
        public string TokenPair { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Token0Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token1Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token0AmountMin { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token1AmountMin { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ExpectedLpTokens { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalValueUsd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PriceImpact { get; set; }

        [Range(0, 200)]
        public decimal EstimatedAPY { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EstimatedDailyFees { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EstimatedMonthlyFees { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EstimatedYearlyFees { get; set; }

        [Range(0, 100)]
        public decimal PoolShare { get; set; }

        [Range(0, 100)]
        public decimal SlippageTolerance { get; set; } = 0.5m;

        public bool IsValid { get; set; }
        public bool HasSufficientBalance { get; set; }
        public bool IsWithinSlippage { get; set; }

        public string Token0Symbol { get; set; } = string.Empty;
        public string Token1Symbol { get; set; } = string.Empty;

        // Risk metrics
        public string RiskLevel { get; set; } = string.Empty;
        public string ImpermanentLossEstimate { get; set; } = string.Empty;

        // Validation messages
        public List<string> ValidationMessages { get; set; } = new();
        public List<string> WarningMessages { get; set; } = new();

        // Formatted displays
        public string TotalValueDisplay { get; set; } = string.Empty;
        public string ApyDisplay { get; set; } = string.Empty;
        public string DailyFeesDisplay { get; set; } = string.Empty;
        public string MonthlyFeesDisplay { get; set; } = string.Empty;
        public string PriceImpactDisplay { get; set; } = string.Empty;
    }
}
