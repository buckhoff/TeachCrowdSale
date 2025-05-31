using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class PriceSummaryModel
    {
        public decimal CurrentPrice { get; set; }
        public decimal PriceChange24h { get; set; }
        public decimal PriceChangePercent24h { get; set; }
        public decimal Volume24h { get; set; }
        public decimal MarketCap { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
