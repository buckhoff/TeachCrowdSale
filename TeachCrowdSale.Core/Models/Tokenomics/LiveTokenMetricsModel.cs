using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Tokenomics
{
    /// <summary>
    /// Live token metrics model
    /// </summary>
    public class LiveTokenMetricsModel
    {
        public decimal CurrentPrice { get; set; }
        public decimal MarketCap { get; set; }
        public decimal Volume24h { get; set; }
        public decimal PriceChange24h { get; set; }
        public decimal PriceChangePercent24h { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
        public long TotalSupply { get; set; } = 5_000_000_000; // 5B total supply
        public long CirculatingSupply { get; set; }
        public int HoldersCount { get; set; }
        public decimal TotalValueLocked { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
