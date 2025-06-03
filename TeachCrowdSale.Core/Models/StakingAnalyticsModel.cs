using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Staking analytics model
    /// </summary>
    public class StakingAnalyticsModel
    {
        public List<StakingTrendDataModel> StakingTrends { get; set; } = new();
        public List<RewardDistributionDataModel> RewardDistribution { get; set; } = new();
        public List<PoolPerformanceModel> PoolPerformance { get; set; } = new();
        public List<TopStakerModel> TopStakers { get; set; } = new();
        public List<SchoolImpactModel> SchoolImpacts { get; set; } = new();
    }
}
