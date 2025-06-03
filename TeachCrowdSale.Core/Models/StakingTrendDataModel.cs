using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Staking trend data for charts
    /// </summary>
    public class StakingTrendDataModel
    {
        public DateTime Date { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalStaked { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveStakers { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RewardsDistributed { get; set; }
    }
}
