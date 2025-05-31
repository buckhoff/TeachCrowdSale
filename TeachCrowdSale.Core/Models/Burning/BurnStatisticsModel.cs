using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Burning
{
    public class BurnStatisticsModel
    {
        public long TotalBurned { get; set; }
        public decimal BurnedPercentage { get; set; }
        public long BurnedLast30Days { get; set; }
        public decimal BurnRate { get; set; }
        public decimal EstimatedAnnualBurn { get; set; }
        public DateTime LastBurnDate { get; set; }
    }
}
