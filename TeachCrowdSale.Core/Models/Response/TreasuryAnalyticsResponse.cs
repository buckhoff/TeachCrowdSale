using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models.Treasury;

namespace TeachCrowdSale.Core.Models.Response
{
    public class TreasuryAnalyticsResponse
    {
        [Range(0, double.MaxValue)]
        public decimal TotalTreasuryValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StabilityFundBalance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OperationalFunds { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ReserveFunds { get; set; }

        // Runway calculations
        public decimal MonthlyBurnRate { get; set; }
        public int OperationalRunwayMonths { get; set; }
        public int TotalRunwayMonths { get; set; }

        public TreasuryOverviewModel Overview { get; set; } = new();
        public List<TreasuryAllocationModel> Allocations { get; set; } = new();
        public TreasuryPerformanceModel Performance { get; set; } = new();

        // Fund allocation breakdown
        public List<FundAllocationResponse> FundAllocations { get; set; } = new();

        // Treasury performance
        public decimal TreasuryGrowthRate { get; set; }
        public decimal FundingEfficiency { get; set; }

        // Risk metrics
        public decimal DiversificationRatio { get; set; }
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High

        // Projections
        public decimal ProjectedRunwayExtension { get; set; }
        public DateTime EstimatedRunwayEndDate { get; set; }
    }
}
