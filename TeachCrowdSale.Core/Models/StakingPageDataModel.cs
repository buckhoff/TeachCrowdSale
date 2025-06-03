using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Comprehensive staking page data model
    /// </summary>
    public class StakingPageDataModel
    {
        public List<StakingPoolDisplayModel> StakingPools { get; set; } = new();
        public StakingStatsModel StakingStats { get; set; } = new();
        public List<SchoolBeneficiaryModel> Schools { get; set; } = new();
        public StakingAnalyticsModel Analytics { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}
