using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{

    /// <summary>
    /// Price history model for web consumption
    /// Maps from PriceHistoryEntry entity
    /// </summary>
    public class PriceHistoryModel
    {
        public int Id { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume { get; set; }

        public DateTime Timestamp { get; set; }

        public string Source { get; set; } = string.Empty;

        public string Pair { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        public decimal PriceChange24h { get; set; }

        public decimal PriceChangePercent24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MarketCap { get; set; }

        [Range(0, double.MaxValue)]
        public decimal High24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Low24h { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
