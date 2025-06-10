using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Model for transaction history items
    /// </summary>
    public class StakingTransactionModel
    {
        public int Id { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // Stake, Unstake, Claim
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int? PoolId { get; set; }
        public string? PoolName { get; set; }
        public decimal? PenaltyAmount { get; set; }
    }
}
