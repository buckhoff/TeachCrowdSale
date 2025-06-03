using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models.Liquidity;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for user liquidity info
    /// </summary>
    public class UserLiquidityInfoModel
    {
        public string WalletAddress { get; set; } = string.Empty;
        public decimal TotalLiquidityValue { get; set; }
        public decimal TotalFeesEarned { get; set; }
        public decimal TotalPnL { get; set; }
        public decimal TotalPnLPercentage { get; set; }
        public int ActivePositions { get; set; }
        public int TotalPositions { get; set; }
        public DateTime FirstPositionDate { get; set; }
        public List<UserLiquidityPositionModel> Positions { get; set; } = new();
        public List<LiquidityTransactionHistoryModel> RecentTransactions { get; set; } = new();
        public UserLiquidityStatsModel Stats { get; set; } = new();
    }
}
