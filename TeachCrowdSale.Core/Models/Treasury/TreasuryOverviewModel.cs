using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Treasury
{
    public class TreasuryOverviewModel
    {
        public decimal TotalValue { get; set; }
        public decimal OperationalRunwayYears { get; set; }
        public decimal MonthlyBurnRate { get; set; }
        public decimal SafetyFundValue { get; set; }
        public decimal StabilityFundValue { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
