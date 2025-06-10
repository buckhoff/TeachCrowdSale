using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{

    /// <summary>
    /// Result model for staking transactions
    /// </summary>
    public class StakingTransactionResponse
    {
        public bool IsSuccess { get; set; }
        public string? TransactionHash { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal? PenaltyAmount { get; set; } // For early withdrawals
        public decimal? RewardAmount { get; set; } // For reward claims
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
