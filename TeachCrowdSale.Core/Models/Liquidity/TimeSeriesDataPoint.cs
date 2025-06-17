using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Model for time series data points
    /// </summary>
    public class TimeSeriesDataPoint
    {
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
