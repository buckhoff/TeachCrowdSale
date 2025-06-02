using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Burning
{
    /// <summary>
    /// Burn statistics model for web consumption
    /// Maps from BurnSnapshot entity
    /// </summary>
    public class BurnStatisticsModel
    {
        public int Id { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalBurned { get; set; }

        [Range(0, 100)]
        public decimal BurnedPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BurnedLast30Days { get; set; }

        [Range(0, 100)]
        public decimal BurnRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EstimatedAnnualBurn { get; set; }

        public DateTime LastBurnDate { get; set; }

        public DateTime Timestamp { get; set; }

        public bool IsLatest { get; set; }
    }
}
