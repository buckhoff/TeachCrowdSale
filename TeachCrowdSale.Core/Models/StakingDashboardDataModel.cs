using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Comprehensive dashboard data model combining all staking information
    /// </summary>
    public class StakingDashboardDataModel
    {
        public StakingStatsModel Stats { get; set; } = new();
        public List<StakingPoolDisplayModel> Pools { get; set; } = new();
        public List<SchoolBeneficiaryModel> Schools { get; set; } = new();
        public UserStakingInfoModel? UserInfo { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsFallbackData { get; set; }
    }
}
