using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class TimeSeriesDataPointResponse
    {
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
        public string? Label { get; set; }
        public string? Category { get; set; }

        // For OHLC data
        public decimal? Open { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Close { get; set; }
        public decimal? Volume { get; set; }
    }
}
