using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class AnalyticsComparisonResponse
    {
        public AnalyticsPeriodResponse Period1 { get; set; } = new();
        public AnalyticsPeriodResponse Period2 { get; set; } = new();
        public AnalyticsChangesResponse Changes { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
