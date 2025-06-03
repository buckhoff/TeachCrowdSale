using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// User's liquidity position tracking
    /// </summary>
    public class UserLiquidityPosition
    {
        public int Id { get; set; }

        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        public int LiquidityPoolId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal LpTokenAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token0Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token1Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal InitialValueUsd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentValueUsd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal FeesEarnedUsd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ImpermanentLoss { get; set; }

        public decimal NetPnL { get; set; } // Can be negative

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RemovedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        [TransactionHash]
        public string? AddTransactionHash { get; set; }

        [TransactionHash]
        public string? RemoveTransactionHash { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public LiquidityPool LiquidityPool { get; set; } = null!;
        public List<LiquidityTransaction> Transactions { get; set; } = new();
    }
}
