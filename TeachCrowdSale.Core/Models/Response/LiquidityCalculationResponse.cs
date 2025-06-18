using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for liquidity calculation results
    /// Calculation data for API consumption
    /// </summary>
    public class LiquidityCalculationResponse
    {
        public int PoolId { get; set; }
        public string TokenPair { get; set; } = string.Empty;
        public string Token0Symbol { get; set; } = string.Empty;
        public string Token1Symbol { get; set; } = string.Empty;

        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        public decimal Token0Amount { get; set; }
        public decimal Token1Amount { get; set; }

        // FIXED: Property name mismatch - method uses EstimatedLpTokens
        public decimal EstimatedLpTokens { get; set; }

        // ADDED: Missing properties referenced in calculation method
        public decimal Token0AmountMin { get; set; }  // MinToken0Amount -> Token0AmountMin
        public decimal Token1AmountMin { get; set; }  // MinToken1Amount -> Token1AmountMin
        public decimal CurrentPrice { get; set; }     // Used in calculation but missing
        public decimal PoolShare { get; set; }        // Used in calculation but missing

        // FIXED: Property name changes for consistency
        public decimal EstimatedValueUsd { get; set; }  // TotalValueUsd -> EstimatedValueUsd

        public decimal SlippageTolerance { get; set; }
        public decimal PriceImpact { get; set; }
        public decimal EstimatedAPY { get; set; }

        // FIXED: Property naming - calculation uses EstimatedDailyFees not EstimatedDailyEarnings
        public decimal EstimatedDailyFees { get; set; }   // EstimatedDailyEarnings -> EstimatedDailyFees
        public decimal EstimatedMonthlyFees { get; set; } // EstimatedMonthlyEarnings -> EstimatedMonthlyFees
        public decimal EstimatedYearlyFees { get; set; }  // EstimatedYearlyEarnings -> EstimatedYearlyFees

        public decimal GasEstimate { get; set; }
        public bool HasSufficientBalance { get; set; }
        public bool HasSufficientAllowance { get; set; }

        // ADDED: Properties used in calculation method
        public bool IsWithinSlippage { get; set; }
        public bool IsValid { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string ImpermanentLossEstimate { get; set; } = string.Empty;

        // ADDED: Display properties used in calculation method
        public string TotalValueDisplay { get; set; } = string.Empty;
        public string ApyDisplay { get; set; } = string.Empty;
        public string DailyFeesDisplay { get; set; } = string.Empty;
        public string MonthlyFeesDisplay { get; set; } = string.Empty;
        public string PriceImpactDisplay { get; set; } = string.Empty;

        public List<string> ValidationMessages { get; set; } = new();
        public List<string> WarningMessages { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}
