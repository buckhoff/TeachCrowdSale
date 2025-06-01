using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class TokenMetricsSnapshot
    {
        public int Id { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketCap { get; set; }
        public decimal Volume24h { get; set; }
        public decimal PriceChange24h { get; set; }
        public decimal PriceChangePercent24h { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
        public decimal TotalSupply { get; set; } = 5_000_000_000;
        public decimal CirculatingSupply { get; set; }
        public int HoldersCount { get; set; }
        public decimal TotalValueLocked { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsLatest { get; set; }
        public string Source { get; set; } = "DEX_AGGREGATOR";
    }
}
