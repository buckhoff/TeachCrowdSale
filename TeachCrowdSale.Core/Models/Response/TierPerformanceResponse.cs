using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Models.Response
{
    public class TierPerformanceResponse
    {
        public int TierId { get; set; }
        public string TierName { get; set; } = string.Empty;

        [Range(0.00000001, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Allocation { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Sold { get; set; }

        public decimal Progress => Allocation > 0 ? (Sold / Allocation) * 100 : 0;

        // Time metrics
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysActive { get; set; }
        public int? DaysToSellOut { get; set; }

        // Performance metrics
        public decimal SalesVelocity { get; set; } // tokens per day
        public decimal RevenueGenerated { get; set; }
        public int ParticipantsCount { get; set; }
        public decimal AverageInvestment { get; set; }

        // Daily/Weekly sales metrics
        [Range(0, double.MaxValue)]
        public decimal SoldToday { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SoldThisWeek { get; set; }

        // Status
        public string Status { get; set; } = string.Empty; // Active, Completed, Upcoming
        public bool IsActive { get; set; }
        public bool IsSoldOut { get; set; }

        // Comparison metrics
        public decimal PerformanceVsPrevious { get; set; }
        public decimal PerformanceVsTarget { get; set; }
    }
}