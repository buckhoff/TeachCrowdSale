using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class AnalyticsMetricResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal? PreviousValue { get; set; }
        public decimal? ChangePercentage { get; set; }
        public string? Unit { get; set; }
        public string? FormattedValue { get; set; }
        public string? TrendDirection { get; set; } // Up, Down, Stable
        public DateTime LastUpdated { get; set; }
        public string? Description { get; set; }
    }
}
