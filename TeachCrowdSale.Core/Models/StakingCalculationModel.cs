using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Staking calculation preview model
    /// </summary>
    public class StakingCalculationModel
    {
        [Range(0, double.MaxValue)]
        public decimal StakeAmount { get; set; }

        [Range(0, 365)]
        public int LockPeriodDays { get; set; }

        [Range(0, 200)]
        public decimal EstimatedAPY { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ProjectedDailyRewards { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ProjectedMonthlyRewards { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ProjectedYearlyRewards { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UserRewardShare { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SchoolRewardShare { get; set; }

        public DateTime UnlockDate { get; set; }
        public bool MeetsMinimum { get; set; }
        public bool WithinMaximum { get; set; }
        public bool IsValid { get; set; }
    }
}
