using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;
using TeachCrowdSale.Core.Data.Enum;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Liquidity add/remove transaction history
    /// </summary>
    public class LiquidityTransaction
    {
        public int Id { get; set; }

        public int UserLiquidityPositionId { get; set; }

        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = string.Empty; // ADD, REMOVE, CLAIM_FEES

        [Range(0, double.MaxValue)]
        public decimal Token0Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Token1Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal LpTokenAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ValueUsd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GasFeesUsd { get; set; }

        [TransactionHash]
        public string TransactionHash { get; set; } = string.Empty;

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public UserLiquidityPosition UserLiquidityPosition { get; set; } = null!;
    }
}
