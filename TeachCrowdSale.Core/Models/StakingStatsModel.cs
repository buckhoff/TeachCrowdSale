using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Overall staking statistics
    /// </summary>
    public class StakingStatsModel
    {
        [Range(0, double.MaxValue)]
        public decimal TotalValueLocked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalRewardsDistributed { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveStakers { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AverageAPY { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalSchoolFunding { get; set; }

        [Range(0, int.MaxValue)]
        public int SchoolsSupported { get; set; }

        [Range(0, 100)]
        public decimal StakingParticipation { get; set; }

        [Range(0, 100)]
        public decimal YearlySchoolFunding { get; set; }
    }
}
