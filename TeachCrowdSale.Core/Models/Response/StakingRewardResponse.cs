using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Model for staking reward calculation results
    /// </summary>
    public class StakingRewardResponse
    {
        public decimal TotalRewards { get; set; }
        public decimal UserRewards { get; set; }
        public decimal SchoolRewards { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}
