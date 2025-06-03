using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for staking service operations
    /// </summary>
    public interface IStakingService
    {
        // Pool Management
        Task<List<StakingPool>> GetActiveStakingPoolsAsync();
        Task<StakingPool?> GetStakingPoolAsync(int poolId);
        Task<StakingStatsModel> GetStakingStatsAsync();

        // User Staking Operations
        Task<List<UserStake>> GetUserStakesAsync(string walletAddress);
        Task<UserStakingInfoModel> GetUserStakingInfoAsync(string walletAddress);
        Task<decimal> CalculateStakingRewardsAsync(string walletAddress, int poolId);
        Task<bool> StakeTokensAsync(string walletAddress, int poolId, decimal amount, int? schoolBeneficiaryId = null);
        Task<bool> UnstakeTokensAsync(string walletAddress, int stakeId);
        Task<bool> ClaimRewardsAsync(string walletAddress, int stakeId);
        Task<bool> CompoundRewardsAsync(string walletAddress, int stakeId);

        // School Beneficiary Operations
        Task<List<SchoolBeneficiary>> GetActiveSchoolsAsync(string? country = null, string? state = null);
        Task<SchoolBeneficiary?> GetSchoolBeneficiaryAsync(int schoolId);
        Task<bool> SelectSchoolBeneficiaryAsync(string walletAddress, int schoolId);
        Task<UserStakingBeneficiary?> GetUserSelectedSchoolAsync(string walletAddress);

        // Reward Calculations
        Task<StakingCalculationModel> CalculateStakingPreviewAsync(string walletAddress, int poolId, decimal amount, int lockPeriodDays);
        Task<List<RewardProjectionModel>> GetRewardProjectionsAsync(string walletAddress, int poolId, decimal amount, int lockPeriodDays);

        // Analytics
        Task<StakingAnalyticsModel> GetStakingAnalyticsAsync();
        Task<List<TopStakerModel>> GetTopStakersAsync(int limit = 10);
        Task<List<SchoolImpactModel>> GetSchoolImpactStatsAsync();
    }
}
