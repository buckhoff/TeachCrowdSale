using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Utility
{
    /// <summary>
    /// Utility metrics model for web consumption
    /// Maps from UtilityMetricsSnapshot entity
    /// </summary>
    public class UtilityMetricsModel
    {
        public int Id { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalUtilityVolume { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveUtilities { get; set; }

        [Range(0, 100)]
        public decimal MonthlyGrowthRate { get; set; }

        [Range(0, long.MaxValue)]
        public long UniqueUsers { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AverageTransactionValue { get; set; }

        public DateTime Timestamp { get; set; }

        public bool IsLatest { get; set; }

        public string Source { get; set; } = "PLATFORM";
    }
}
