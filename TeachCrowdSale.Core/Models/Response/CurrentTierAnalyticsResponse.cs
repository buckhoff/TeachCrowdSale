using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
   public class CurrentTierAnalyticsResponse
    {
        public int TierId { get; set; }
        public string TierName { get; set; } = string.Empty;

        [Range(0.00000001, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Allocation { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Sold { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Remaining { get; set; }

        public decimal Progress => Allocation > 0 ? (Sold / Allocation) * 100 : 0;

        // Performance metrics
        public decimal SoldToday { get; set; }
        public decimal SoldThisWeek { get; set; }
        public decimal SalesVelocity { get; set; } // tokens per day
        public int EstimatedDaysToSellOut { get; set; }

        // Conversion metrics
        public decimal ConversionRate { get; set; } // visitors to purchasers
        public int PageViews24h { get; set; }
        public int UniquePurchasers24h { get; set; }
    }
}
