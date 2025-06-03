using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interface for staking repository operations
    /// </summary>
    public interface IStakingRepository
    {
        // Staking Pools
        Task<List<StakingPool>> GetActiveStakingPoolsAsync();
        Task<StakingPool?> GetStakingPoolByIdAsync(int poolId);
        Task<StakingPool> UpdateStakingPoolAsync(StakingPool pool);

        // User Stakes
        Task<List<UserStake>> GetUserStakesAsync(string walletAddress, bool activeOnly = false);
        Task<UserStake?> GetUserStakeByIdAsync(int stakeId);
        Task<UserStake> CreateUserStakeAsync(UserStake stake);
        Task<UserStake> UpdateUserStakeAsync(UserStake stake);

        // Reward Claims
        Task<List<StakingRewardClaim>> GetUserRewardClaimsAsync(string walletAddress);
        Task<StakingRewardClaim> CreateRewardClaimAsync(StakingRewardClaim claim);
        Task<StakingRewardClaim> UpdateRewardClaimAsync(StakingRewardClaim claim);

        // School Beneficiaries
        Task<List<SchoolBeneficiary>> GetActiveSchoolsAsync(string? country = null, string? state = null);
        Task<SchoolBeneficiary?> GetSchoolBeneficiaryByIdAsync(int schoolId);
        Task<UserStakingBeneficiary?> GetUserSelectedSchoolAsync(string walletAddress);
        Task<UserStakingBeneficiary> CreateOrUpdateUserSchoolSelectionAsync(UserStakingBeneficiary selection);

        // School Distributions
        Task<List<SchoolRewardDistribution>> GetSchoolDistributionsAsync(int schoolId);
        Task<SchoolRewardDistribution> CreateSchoolDistributionAsync(SchoolRewardDistribution distribution);

        // Analytics
        Task<decimal> GetTotalStakedAsync();
        Task<decimal> GetTotalRewardsDistributedAsync();
        Task<int> GetActiveStakersCountAsync();
        Task<List<TopStakerModel>> GetTopStakersAsync(int limit);
    }
}
