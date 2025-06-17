using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for user liquidity position data
    /// Maps from UserLiquidityPosition entity for API consumption
    /// </summary>
    public class UserLiquidityPositionResponse
    {
        public int Id { get; set; }
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public string Token0Symbol { get; set; } = string.Empty;
        public string Token1Symbol { get; set; } = string.Empty;
        public string DexName { get; set; } = string.Empty;

        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        public decimal LpTokenAmount { get; set; }
        public decimal Token0Amount { get; set; }
        public decimal Token1Amount { get; set; }
        public decimal InitialValueUsd { get; set; }
        public decimal CurrentValueUsd { get; set; }
        public decimal FeesEarnedUsd { get; set; }
        public decimal ImpermanentLoss { get; set; }
        public decimal NetPnL { get; set; }
        public decimal PnLPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
