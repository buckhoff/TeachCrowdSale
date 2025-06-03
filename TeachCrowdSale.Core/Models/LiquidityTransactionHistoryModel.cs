using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Transaction history model
    /// </summary>
    public class LiquidityTransactionHistoryModel
    {
        public int Id { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public decimal ValueUsd { get; set; }
        public DateTime Timestamp { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FormattedValue { get; set; } = string.Empty;
        public string FormattedDate { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }
}
