using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Staking pool configuration entity
    /// </summary>
    public class StakingPool
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal MinStakeAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxStakeAmount { get; set; }

        [Range(0, 365)]
        public int LockPeriodDays { get; set; }

        [Range(0, 200)]
        public decimal BaseAPY { get; set; }

        [Range(0, 200)]
        public decimal BonusAPY { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalStaked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxPoolSize { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public List<UserStake> UserStakes { get; set; } = new();
    }
}
