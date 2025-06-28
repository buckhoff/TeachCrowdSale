using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Staking reward claim history
    /// </summary>
    public class StakingRewardClaim
    {
        public int Id { get; set; }
        public int UserStakeId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ClaimedAmount { get; set; }

        public DateTime ClaimDate { get; set; }

        [TransactionHash]
        public string TransactionHash { get; set; } = string.Empty;

        public Enum.TransactionStatus Status { get; set; }

        // Navigation properties
        public UserStake UserStake { get; set; }
    }
}
