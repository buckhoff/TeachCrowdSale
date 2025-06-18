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
        public string TransactionType { get; set; } = string.Empty;
        public string TransactionHash { get; set; } = string.Empty;
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public decimal Token0Amount { get; set; }
        public decimal Token1Amount { get; set; }
        public decimal ValueUsd { get; set; }
        public decimal GasFeesUsd { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string DexName { get; set; } = string.Empty;
    }
}
