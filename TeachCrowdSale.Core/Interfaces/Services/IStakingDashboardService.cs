using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Web service for staking dashboard operations via HttpClient to API
    /// </summary>
    public interface IStakingDashboardService
    {
        // Dashboard Overview
        Task<StakingStatsModel?> GetStakingStatsAsync();
        Task<StakingDashboardDataModel?> GetDashboardDataAsync(string? walletAddress = null);

        // Pool Management
        Task<List<StakingPool>?> GetActiveStakingPoolsAsync();
        Task<StakingPool?> GetStakingPoolAsync(int poolId);

        // User Data
        Task<UserStakingInfoModel?> GetUserStakingInfoAsync(string walletAddress);
        Task<List<UserStakePositionModel>?> GetUserActivePositionsAsync(string walletAddress);
        Task<List<StakingTransactionResponse>?> GetUserTransactionHistoryAsync(string walletAddress, int page = 1, int pageSize = 20);

        // Calculations
        Task<StakingCalculationModel?> CalculateStakingPreviewAsync(string walletAddress, int poolId, decimal amount, int lockPeriodDays);
        Task<List<RewardProjectionModel>?> GetRewardProjectionsAsync(string walletAddress, int poolId, decimal amount, int days = 365);
        Task<decimal?> CalculateStakingRewardsAsync(string walletAddress, int poolId);

        // School Management
        Task<List<SchoolBeneficiaryModel>?> GetAvailableSchoolsAsync();
        Task<SchoolBeneficiaryModel?> GetSchoolDetailsAsync(int schoolId);

        // Transaction Operations
        Task<StakingTransactionResponse?> StakeTokensAsync(StakeTokensRequest request);
        Task<StakingTransactionResponse?> ClaimRewardsAsync(ClaimRewardsRequest request);
        Task<StakingTransactionResponse?> UnstakeTokensAsync(UnstakeTokensRequest request);

        // Cache Management
        void ClearUserCache(string walletAddress);
        void ClearAllCache();

    }
}
