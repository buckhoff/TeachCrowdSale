using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for comprehensive user liquidity information
    /// User-specific data for API consumption
    /// </summary>
    public class UserLiquidityInfoResponse
    {
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;
        public decimal TotalLiquidityValue { get; set; }
        public decimal TotalFeesEarned { get; set; }
        public decimal TotalPnL { get; set; }
        public decimal TotalPnLPercentage { get; set; }
        public int ActivePositions { get; set; }
        public int TotalPositions { get; set; }
        public DateTime FirstPositionDate { get; set; }
        public List<UserLiquidityPositionResponse> Positions { get; set; } = new();
        public List<LiquidityTransactionHistoryResponse> RecentTransactions { get; set; } = new();
        public UserLiquidityStatsResponse Stats { get; set; } = new();
    }
}
