using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class StakingSettings
    {
        public string MinStakeAmount { get; set; } = string.Empty;
        public string MaxStakeAmount { get; set; } = string.Empty;
        public int DefaultLockPeriodDays { get; set; } = 30;
        public decimal EarlyUnstakePenaltyPercentage { get; set; } = 10.0m;
        public int RewardCalculationIntervalHours { get; set; } = 24;
        public List<StakingPool> Pools { get; set; } = new();
    }

    public class StakingPool
    {
        public int PoolId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int LockPeriodDays { get; set; }
        public decimal APYPercentage { get; set; }
        public string MinStake { get; set; } = string.Empty;
        public string MaxStake { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
