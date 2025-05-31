using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Treasury
{
    public class TreasuryMetricTrend
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Metric { get; set; } = string.Empty;
    }
}
