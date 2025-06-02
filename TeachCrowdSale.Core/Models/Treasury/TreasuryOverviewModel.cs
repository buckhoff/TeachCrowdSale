using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Treasury
{
    /// <summary>
    /// Treasury overview model for web consumption
    /// Maps from TreasurySnapshot entity
    /// </summary>
    public class TreasuryOverviewModel
    {
        public int Id { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OperationalRunwayYears { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MonthlyBurnRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SafetyFundValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StabilityFundValue { get; set; }

        public DateTime LastUpdate { get; set; }

        public DateTime Timestamp { get; set; }

        public bool IsLatest { get; set; }

        public string Source { get; set; } = "CALCULATED";
    }
}
