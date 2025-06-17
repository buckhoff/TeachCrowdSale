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
        public decimal EstimatedLpTokens { get; set; }
        public decimal EstimatedValueUsd { get; set; }
        public decimal SlippageTolerance { get; set; }
        public decimal PriceImpact { get; set; }
        public decimal EstimatedAPY { get; set; }
        public decimal EstimatedDailyEarnings { get; set; }
        public decimal EstimatedMonthlyEarnings { get; set; }
        public decimal EstimatedYearlyEarnings { get; set; }
        public decimal MinToken0Amount { get; set; }
        public decimal MinToken1Amount { get; set; }
        public decimal GasEstimate { get; set; }
        public bool HasSufficientBalance { get; set; }
        public bool HasSufficientAllowance { get; set; }
        public List<string> ValidationMessages { get; set; } = new();
        public List<string> WarningMessages { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }
}
