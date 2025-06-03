using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models.Treasury;

namespace TeachCrowdSale.Core.Models.Response
{
    public class AnalyticsDashboardResponse
    {
        public TokenAnalyticsResponse TokenAnalytics { get; set; } = new();
        public PresaleAnalyticsResponse PresaleAnalytics { get; set; } = new();
        public PlatformAnalyticsResponse PlatformAnalytics { get; set; } = new();
        public TreasuryAnalyticsResponse TreasuryAnalytics { get; set; } = new();
        public List<TimeSeriesDataPointResponse> PriceHistory { get; set; } = new();
        public List<TimeSeriesDataPointResponse> VolumeHistory { get; set; } = new();
        public List<TierPerformanceResponse> TierPerformance { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}
