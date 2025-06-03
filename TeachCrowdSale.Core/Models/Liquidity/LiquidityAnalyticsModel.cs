using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Comprehensive liquidity analytics
    /// </summary>
    public class LiquidityAnalyticsModel
    {
        public List<LiquidityTrendDataModel> TvlTrends { get; set; } = new();
        public List<VolumeTrendDataModel> VolumeTrends { get; set; } = new();
        public List<PoolPerformanceDataModel> PoolPerformance { get; set; } = new();
        public List<UserLiquidityStatsModel> TopProviders { get; set; } = new();
        public List<DexComparisonModel> DexComparison { get; set; } = new();
        public LiquidityStatsOverviewModel Overview { get; set; } = new();
    }
}
