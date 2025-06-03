using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class TokenAnalyticsResponse
    {
        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MarketCap { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        public decimal PriceChange24h { get; set; }
        public decimal PriceChange7d { get; set; }
        public decimal PriceChange30d { get; set; }

        [Range(0, int.MaxValue)]
        public int HoldersCount { get; set; }

        public decimal HoldersChange24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalSupply { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CirculatingSupply { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BurnedTokens { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StakedTokens { get; set; }

        // All-time metrics
        public decimal AllTimeHigh { get; set; }
        public DateTime AllTimeHighDate { get; set; }
        public decimal AllTimeLow { get; set; }
        public DateTime AllTimeLowDate { get; set; }

        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
        public decimal TotalValueLocked { get; set; }

        // Distribution metrics
        public decimal LiquidityTokens { get; set; }
        public decimal TreasuryTokens { get; set; }
    }
}
