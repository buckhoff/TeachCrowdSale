using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Liquidity transaction history for API response
    /// </summary>
    public class LiquidityTransactionHistoryResponse
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Add", "Remove", "Claim"
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal ValueUsd { get; set; }

        [TransactionHash]
        public string TransactionHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
