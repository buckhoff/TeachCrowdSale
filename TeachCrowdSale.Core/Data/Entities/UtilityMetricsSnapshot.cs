using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class UtilityMetricsSnapshot
    {
        public int Id { get; set; }
        public decimal TotalUtilityVolume { get; set; }
        public int ActiveUtilities { get; set; }
        public decimal MonthlyGrowthRate { get; set; }
        public long UniqueUsers { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsLatest { get; set; }
        public string Source { get; set; } = "PLATFORM";
    }
}
