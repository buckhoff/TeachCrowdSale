using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for liquidity analytics data
    /// Historical and trend data for API consumption
    /// </summary>
    public class LiquidityAnalyticsResponse
    {
        public List<LiquidityTrendDataResponse> TvlTrends { get; set; } = new();
        public List<VolumeTrendDataResponse> VolumeTrends { get; set; } = new();
        public List<PoolPerformanceDataResponse> PoolPerformance { get; set; } = new();
        public List<UserLiquidityStatsResponse> TopProviders { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }
}
