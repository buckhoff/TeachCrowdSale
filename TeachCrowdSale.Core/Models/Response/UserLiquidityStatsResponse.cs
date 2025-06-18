using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// User liquidity statistics for API response
    /// </summary>
    public class UserLiquidityStatsResponse
    {
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        // ADDED: Display address for UI (shortened version)
        public string DisplayAddress { get; set; } = string.Empty;

        public decimal TotalLiquidityValue { get; set; }

        // ADDED: Total value provided (initial investment)
        public decimal TotalValueProvided { get; set; }

        public decimal TotalFeesEarned { get; set; }
        public decimal TotalPnL { get; set; }
        public int ActivePositions { get; set; }
        public DateTime FirstPositionDate { get; set; }

        // Additional helpful properties for ranking
        public int Rank { get; set; }
        public decimal PnLPercentage { get; set; }
        public TimeSpan TimeActive { get; set; }
    }
}

