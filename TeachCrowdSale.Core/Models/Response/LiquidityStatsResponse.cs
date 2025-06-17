using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for liquidity statistics
    /// Aggregated data for API consumption
    /// </summary>
    public class LiquidityStatsResponse
    {
        public decimal TotalValueLocked { get; set; }
        public decimal TotalVolume24h { get; set; }
        public decimal TotalFeesEarned { get; set; }
        public int ActivePools { get; set; }
        public int TotalLiquidityProviders { get; set; }
        public decimal AverageAPY { get; set; }
        public decimal TeachPrice { get; set; }
        public string PriceChangeDisplay { get; set; } = string.Empty;
        public string PriceChangeClass { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}
