using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Utility
{
    public class UtilityMetricsModel
    {
        public decimal TotalUtilityVolume { get; set; }
        public int ActiveUtilities { get; set; }
        public decimal MonthlyGrowthRate { get; set; }
        public long UniqueUsers { get; set; }
        public decimal AverageTransactionValue { get; set; }
    }
}
