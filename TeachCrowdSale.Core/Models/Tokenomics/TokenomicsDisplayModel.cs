using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Models.Burning;
using TeachCrowdSale.Core.Models.Governance;
using TeachCrowdSale.Core.Models.Treasury;
using TeachCrowdSale.Core.Models.Utility;
using TeachCrowdSale.Core.Models.Vesting;

namespace TeachCrowdSale.Core.Models.Tokenomics
{
    /// <summary>
    /// Main tokenomics page data model
    /// </summary>
    public class TokenomicsDisplayModel
    {
        public TokenMetricsModel LiveMetrics { get; set; } = new();
        public SupplyBreakdownModel SupplyBreakdown { get; set; } = new();
        public VestingScheduleModel VestingSchedule { get; set; } = new();
        public BurnMechanicsModel BurnMechanics { get; set; } = new();
        public TreasuryAnalyticsModel TreasuryAnalytics { get; set; } = new();
        public UtilityFeaturesModel UtilityFeatures { get; set; } = new();
        public GovernanceInfoModel GovernanceInfo { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}
