using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Treasury
{
    public class TreasuryAnalyticsModel
    {
        public TreasuryOverviewModel Overview { get; set; } = new();
        public List<TreasuryAllocationModel> Allocations { get; set; } = new();
        public TreasuryPerformanceModel Performance { get; set; } = new();
        public List<TreasuryScenarioModel> Scenarios { get; set; } = new();
    }
}
