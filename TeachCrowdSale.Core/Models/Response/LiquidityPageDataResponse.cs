using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for comprehensive liquidity page data
    /// Aggregated data for API consumption
    /// </summary>
    public class LiquidityPageDataResponse
    {
        public List<LiquidityPoolResponse> LiquidityPools { get; set; } = new();
        public List<DexConfigurationResponse> DexOptions { get; set; } = new();
        public LiquidityStatsResponse Stats { get; set; } = new();
        public LiquidityAnalyticsResponse Analytics { get; set; } = new();
        public List<LiquidityGuideStepResponse> GuideSteps { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}
