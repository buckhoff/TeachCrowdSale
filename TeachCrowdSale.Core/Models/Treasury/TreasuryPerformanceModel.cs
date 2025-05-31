using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Treasury
{
    public class TreasuryPerformanceModel
    {
        public decimal EfficiencyRating { get; set; }
        public decimal CostPerUser { get; set; }
        public decimal RevenueGrowthRate { get; set; }
        public decimal SustainabilityScore { get; set; }
        public List<TreasuryMetricTrend> Trends { get; set; } = new();
    }
}
