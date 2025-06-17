using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Comprehensive liquidity page data model
    /// </summary>
    public class LiquidityPageDataModel
    {
        public List<LiquidityPoolModel> LiquidityPools { get; set; } = new();
        public List<DexConfigurationModel> DexOptions { get; set; } = new();
        public LiquidityStatsModel Stats { get; set; } = new();
        public LiquidityAnalyticsModel Analytics { get; set; } = new();
        public List<LiquidityGuideStepModel> GuideSteps { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}
