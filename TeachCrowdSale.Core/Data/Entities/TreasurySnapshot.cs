using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class TreasurySnapshot
    {
        public int Id { get; set; }
        public decimal TotalValue { get; set; }
        public decimal OperationalRunwayYears { get; set; }
        public decimal MonthlyBurnRate { get; set; }
        public decimal SafetyFundValue { get; set; }
        public decimal StabilityFundValue { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsLatest { get; set; }
        public string Source { get; set; } = "CALCULATED";
    }
}
