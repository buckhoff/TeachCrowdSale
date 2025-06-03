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
    /// User staking position entity
    /// </summary>
    public class UserStake
    {
        public int Id { get; set; }

        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        public int StakingPoolId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StakedAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AccruedRewards { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ClaimedRewards { get; set; }

        public DateTime StakeDate { get; set; }
        public DateTime? UnstakeDate { get; set; }
        public DateTime LastRewardCalculation { get; set; }
        public DateTime? LastClaimDate { get; set; }

        public bool IsActive { get; set; }

        [TransactionHash]
        public string? StakeTransactionHash { get; set; }

        [TransactionHash]
        public string? UnstakeTransactionHash { get; set; }

        // Navigation properties
        public StakingPool StakingPool { get; set; }
        public List<StakingRewardClaim> RewardClaims { get; set; } = new();
    }

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
