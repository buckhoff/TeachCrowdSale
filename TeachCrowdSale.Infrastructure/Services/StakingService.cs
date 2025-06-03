using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Infrastructure.Services;

/// <summary>
/// Service implementation for staking operations
/// </summary>
public class StakingService : IStakingService
{
    private readonly ILogger<StakingService> _logger;
    private readonly IStakingRepository _stakingRepository;
    private readonly IStakingContractService _contractService;
    private readonly IBlockchainService _blockchainService;

    public StakingService(
        ILogger<StakingService> logger,
        IStakingRepository stakingRepository,
        IStakingContractService contractService,
        IBlockchainService blockchainService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stakingRepository = stakingRepository ?? throw new ArgumentNullException(nameof(stakingRepository));
        _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
        _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
    }

    #region Pool Management

    public async Task<List<StakingPool>> GetActiveStakingPoolsAsync()
    {
        try
        {
            return await _stakingRepository.GetActiveStakingPoolsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active staking pools");
            throw;
        }
    }

    public async Task<StakingPool?> GetStakingPoolAsync(int poolId)
    {
        try
        {
            return await _stakingRepository.GetStakingPoolByIdAsync(poolId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staking pool {PoolId}", poolId);
            throw;
        }
    }

    public async Task<StakingStatsModel> GetStakingStatsAsync()
    {
        try
        {
            var totalStaked = await _stakingRepository.GetTotalStakedAsync();
            var totalRewards = await _stakingRepository.GetTotalRewardsDistributedAsync();
            var activeStakers = await _stakingRepository.GetActiveStakersCountAsync();
            var pools = await _stakingRepository.GetActiveStakingPoolsAsync();

            // Calculate average APY weighted by pool size
            var weightedAPY = pools.Where(p => p.TotalStaked > 0)
                .Sum(p => (p.BaseAPY + p.BonusAPY) * p.TotalStaked) /
                Math.Max(totalStaked, 1);

            // Get school funding stats
            var schoolStats = await GetSchoolFundingStatsAsync();

            return new StakingStatsModel
            {
                TotalValueLocked = totalStaked,
                TotalRewardsDistributed = totalRewards,
                ActiveStakers = activeStakers,
                AverageAPY = weightedAPY,
                TotalSchoolFunding = schoolStats.TotalFunding,
                SchoolsSupported = schoolStats.SchoolsSupported,
                StakingParticipation = CalculateStakingParticipation(totalStaked)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staking statistics");
            throw;
        }
    }

    #endregion

    #region User Staking Operations

    public async Task<List<UserStake>> GetUserStakesAsync(string walletAddress)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(walletAddress))
            {
                throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
            }

            return await _stakingRepository.GetUserStakesAsync(walletAddress, activeOnly: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user stakes for {WalletAddress}", walletAddress);
            throw;
        }
    }

    public async Task<UserStakingInfoModel> GetUserStakingInfoAsync(string walletAddress)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(walletAddress))
            {
                throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
            }

            var userStakes = await _stakingRepository.GetUserStakesAsync(walletAddress);
            var selectedSchool = await _stakingRepository.GetUserSelectedSchoolAsync(walletAddress);
            var rewardHistory = await _stakingRepository.GetUserRewardClaimsAsync(walletAddress);

            var totalStaked = userStakes.Where(s => s.IsActive).Sum(s => s.StakedAmount);
            var totalRewards = userStakes.Sum(s => s.ClaimedRewards);
            var claimableRewards = userStakes.Where(s => s.IsActive).Sum(s => s.AccruedRewards);

            // Calculate projected monthly rewards
            var projectedMonthly = await CalculateProjectedMonthlyRewards(userStakes.Where(s => s.IsActive));

            return new UserStakingInfoModel
            {
                WalletAddress = walletAddress,
                TotalStaked = totalStaked,
                TotalRewards = totalRewards,
                ClaimableRewards = claimableRewards,
                ProjectedMonthlyRewards = projectedMonthly,
                StakePositions = await MapToStakePositionModels(userStakes),
                SelectedSchool = selectedSchool != null ? MapToSchoolBeneficiaryModel(selectedSchool.SchoolBeneficiary) : null,
                RewardHistory = MapToRewardHistoryModels(rewardHistory)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user staking info for {WalletAddress}", walletAddress);
            throw;
        }
    }

    public async Task<decimal> CalculateStakingRewardsAsync(string walletAddress, int poolId)
    {
        try
        {
            var userStakes = await _stakingRepository.GetUserStakesAsync(walletAddress, activeOnly: true);
            var poolStakes = userStakes.Where(s => s.StakingPoolId == poolId);

            decimal totalRewards = 0;
            foreach (var stake in poolStakes)
            {
                var pendingRewards = await _contractService.GetPendingRewardsAsync(walletAddress, stake.Id);
                totalRewards += pendingRewards;
            }

            return totalRewards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating staking rewards for {WalletAddress} in pool {PoolId}", walletAddress, poolId);
            throw;
        }
    }

    public async Task<bool> StakeTokensAsync(string walletAddress, int poolId, decimal amount, int? schoolBeneficiaryId = null)
    {
        try
        {
            // Validate inputs
            if (!_blockchainService.IsValidAddress(walletAddress))
            {
                throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
            }

            var pool = await _stakingRepository.GetStakingPoolByIdAsync(poolId);
            if (pool == null || !pool.IsActive)
            {
                throw new InvalidOperationException($"Staking pool {poolId} is not available");
            }

            if (amount < pool.MinStakeAmount || amount > pool.MaxStakeAmount)
            {
                throw new InvalidOperationException($"Stake amount must be between {pool.MinStakeAmount} and {pool.MaxStakeAmount}");
            }

            // Execute blockchain transaction
            var transactionHash = await _contractService.StakeTokensTransactionAsync(walletAddress, poolId, amount);

            if (string.IsNullOrEmpty(transactionHash))
            {
                return false;
            }

            // Create database record
            var userStake = new UserStake
            {
                WalletAddress = walletAddress.ToLowerInvariant(),
                StakingPoolId = poolId,
                StakedAmount = amount,
                AccruedRewards = 0,
                ClaimedRewards = 0,
                StakeTransactionHash = transactionHash
            };

            await _stakingRepository.CreateUserStakeAsync(userStake);

            // Update pool total
            pool.TotalStaked += amount;
            await _stakingRepository.UpdateStakingPoolAsync(pool);

            // Set school beneficiary if provided
            if (schoolBeneficiaryId.HasValue)
            {
                await SelectSchoolBeneficiaryAsync(walletAddress, schoolBeneficiaryId.Value);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error staking tokens for {WalletAddress}", walletAddress);
            throw;
        }
    }

    public async Task<bool> UnstakeTokensAsync(string walletAddress, int stakeId)
    {
        try
        {
            var stake = await _stakingRepository.GetUserStakeByIdAsync(stakeId);
            if (stake == null || stake.WalletAddress != walletAddress.ToLowerInvariant() || !stake.IsActive)
            {
                throw new InvalidOperationException("Invalid stake or unauthorized access");
            }

            // Check if stake is unlocked
            var unlockTime = await _contractService.GetStakeUnlockTimeAsync(stakeId);
            if (DateTime.UtcNow < unlockTime)
            {
                throw new InvalidOperationException($"Stake is locked until {unlockTime:yyyy-MM-dd HH:mm:ss}");
            }

            // Execute blockchain transaction
            var transactionHash = await _contractService.UnstakeTokensTransactionAsync(walletAddress, stakeId);

            if (string.IsNullOrEmpty(transactionHash))
            {
                return false;
            }

            // Update database record
            stake.IsActive = false;
            stake.UnstakeDate = DateTime.UtcNow;
            stake.UnstakeTransactionHash = transactionHash;

            await _stakingRepository.UpdateUserStakeAsync(stake);

            // Update pool total
            if (stake.StakingPool != null)
            {
                stake.StakingPool.TotalStaked -= stake.StakedAmount;
                await _stakingRepository.UpdateStakingPoolAsync(stake.StakingPool);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unstaking tokens for {WalletAddress}, stake {StakeId}", walletAddress, stakeId);
            throw;
        }
    }

    public async Task<bool> ClaimRewardsAsync(string walletAddress, int stakeId)
    {
        try
        {
            var stake = await _stakingRepository.GetUserStakeByIdAsync(stakeId);
            if (stake == null || stake.WalletAddress != walletAddress.ToLowerInvariant() || !stake.IsActive)
            {
                throw new InvalidOperationException("Invalid stake or unauthorized access");
            }

            // Get pending rewards
            var pendingRewards = await _contractService.GetPendingRewardsAsync(walletAddress, stakeId);
            if (pendingRewards <= 0)
            {
                throw new InvalidOperationException("No rewards available to claim");
            }

            // Execute blockchain transaction
            var transactionHash = await _contractService.ClaimRewardsTransactionAsync(walletAddress, stakeId);

            if (string.IsNullOrEmpty(transactionHash))
            {
                return false;
            }

            // Create claim record
            var claim = new StakingRewardClaim
            {
                UserStakeId = stakeId,
                ClaimedAmount = pendingRewards,
                TransactionHash = transactionHash,
                Status = TransactionStatus.Confirmed
            };

            await _stakingRepository.CreateRewardClaimAsync(claim);

            // Update stake record
            stake.ClaimedRewards += pendingRewards;
            stake.AccruedRewards = 0;
            stake.LastClaimDate = DateTime.UtcNow;
            await _stakingRepository.UpdateUserStakeAsync(stake);

            // Distribute school rewards (50% of user rewards)
            await DistributeSchoolRewardsAsync(walletAddress, pendingRewards * 0.5m, transactionHash);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming rewards for {WalletAddress}, stake {StakeId}", walletAddress, stakeId);
            throw;
        }
    }

    public async Task<bool> CompoundRewardsAsync(string walletAddress, int stakeId)
    {
        try
        {
            var stake = await _stakingRepository.GetUserStakeByIdAsync(stakeId);
            if (stake == null || stake.WalletAddress != walletAddress.ToLowerInvariant() || !stake.IsActive)
            {
                throw new InvalidOperationException("Invalid stake or unauthorized access");
            }

            // Get pending rewards
            var pendingRewards = await _contractService.GetPendingRewardsAsync(walletAddress, stakeId);
            if (pendingRewards <= 0)
            {
                throw new InvalidOperationException("No rewards available to compound");
            }

            // Execute blockchain transaction
            var transactionHash = await _contractService.CompoundRewardsTransactionAsync(walletAddress, stakeId);

            if (string.IsNullOrEmpty(transactionHash))
            {
                return false;
            }

            // Update stake record - add rewards to staked amount
            stake.StakedAmount += pendingRewards;
            stake.AccruedRewards = 0;
            stake.LastRewardCalculation = DateTime.UtcNow;
            await _stakingRepository.UpdateUserStakeAsync(stake);

            // Update pool total
            if (stake.StakingPool != null)
            {
                stake.StakingPool.TotalStaked += pendingRewards;
                await _stakingRepository.UpdateStakingPoolAsync(stake.StakingPool);
            }

            // Distribute school rewards (50% of compounded amount)
            await DistributeSchoolRewardsAsync(walletAddress, pendingRewards * 0.5m, transactionHash);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compounding rewards for {WalletAddress}, stake {StakeId}", walletAddress, stakeId);
            throw;
        }
    }

    #endregion

    #region School Beneficiary Operations

    public async Task<List<SchoolBeneficiary>> GetActiveSchoolsAsync(string? country = null, string? state = null)
    {
        try
        {
            return await _stakingRepository.GetActiveSchoolsAsync(country, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active schools");
            throw;
        }
    }

    public async Task<SchoolBeneficiary?> GetSchoolBeneficiaryAsync(int schoolId)
    {
        try
        {
            return await _stakingRepository.GetSchoolBeneficiaryByIdAsync(schoolId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving school beneficiary {SchoolId}", schoolId);
            throw;
        }
    }

    public async Task<bool> SelectSchoolBeneficiaryAsync(string walletAddress, int schoolId)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(walletAddress))
            {
                throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
            }

            var school = await _stakingRepository.GetSchoolBeneficiaryByIdAsync(schoolId);
            if (school == null)
            {
                throw new InvalidOperationException($"School beneficiary {schoolId} not found");
            }

            var selection = new UserStakingBeneficiary
            {
                WalletAddress = walletAddress.ToLowerInvariant(),
                SchoolBeneficiaryId = schoolId
            };

            await _stakingRepository.CreateOrUpdateUserSchoolSelectionAsync(selection);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting school beneficiary for {WalletAddress}", walletAddress);
            throw;
        }
    }

    public async Task<UserStakingBeneficiary?> GetUserSelectedSchoolAsync(string walletAddress)
    {
        try
        {
            if (!_blockchainService.IsValidAddress(walletAddress))
            {
                throw new ArgumentException("Invalid wallet address", nameof(walletAddress));
            }

            return await _stakingRepository.GetUserSelectedSchoolAsync(walletAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving selected school for {WalletAddress}", walletAddress);
            throw;
        }
    }

    #endregion

    #region Reward Calculations

    public async Task<StakingCalculationModel> CalculateStakingPreviewAsync(string walletAddress, int poolId, decimal amount, int lockPeriodDays)
    {
        try
        {
            var pool = await _stakingRepository.GetStakingPoolByIdAsync(poolId);
            if (pool == null)
            {
                throw new InvalidOperationException($"Staking pool {poolId} not found");
            }

            var totalAPY = pool.BaseAPY + pool.BonusAPY;
            var dailyRate = totalAPY / 365 / 100;

            var projectedDaily = amount * dailyRate;
            var projectedMonthly = projectedDaily * 30;
            var projectedYearly = amount * (totalAPY / 100);

            var userShare = projectedDaily * 0.5m; // 50% to user
            var schoolShare = projectedDaily * 0.5m; // 50% to school

            return new StakingCalculationModel
            {
                StakeAmount = amount,
                LockPeriodDays = lockPeriodDays,
                EstimatedAPY = totalAPY,
                ProjectedDailyRewards = projectedDaily,
                ProjectedMonthlyRewards = projectedMonthly,
                ProjectedYearlyRewards = projectedYearly,
                UserRewardShare = userShare,
                SchoolRewardShare = schoolShare,
                UnlockDate = DateTime.UtcNow.AddDays(lockPeriodDays),
                MeetsMinimum = amount >= pool.MinStakeAmount,
                WithinMaximum = amount <= pool.MaxStakeAmount,
                IsValid = amount >= pool.MinStakeAmount && amount <= pool.MaxStakeAmount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating staking preview");
            throw;
        }
    }

    public async Task<List<RewardProjectionModel>> GetRewardProjectionsAsync(string walletAddress, int poolId, decimal amount, int lockPeriodDays)
    {
        try
        {
            var calculation = await CalculateStakingPreviewAsync(walletAddress, poolId, amount, lockPeriodDays);
            var projections = new List<RewardProjectionModel>();

            var currentDate = DateTime.UtcNow;
            var cumulativeRewards = 0m;
            var compoundedAmount = amount;

            for (int day = 1; day <= Math.Min(lockPeriodDays, 365); day++)
            {
                var dailyRewards = calculation.ProjectedDailyRewards;
                cumulativeRewards += dailyRewards;
                compoundedAmount += dailyRewards;

                if (day % 7 == 0) // Weekly snapshots
                {
                    projections.Add(new RewardProjectionModel
                    {
                        Date = currentDate.AddDays(day),
                        CumulativeRewards = cumulativeRewards,
                        PeriodRewards = dailyRewards * 7,
                        CompoundedAmount = compoundedAmount
                    });
                }
            }

            return projections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating reward projections");
            throw;
        }
    }

    #endregion

    #region Analytics

    public async Task<StakingAnalyticsModel> GetStakingAnalyticsAsync()
    {
        try
        {
            var stakingTrends = await GetStakingTrendsAsync();
            var rewardDistribution = await GetRewardDistributionAsync();
            var poolPerformance = await GetPoolPerformanceAsync();
            var topStakers = await _stakingRepository.GetTopStakersAsync(10);
            var schoolImpacts = await GetSchoolImpactStatsAsync();

            return new StakingAnalyticsModel
            {
                StakingTrends = stakingTrends,
                RewardDistribution = rewardDistribution,
                PoolPerformance = poolPerformance,
                TopStakers = topStakers,
                SchoolImpacts = schoolImpacts
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staking analytics");
            throw;
        }
    }

    public async Task<List<TopStakerModel>> GetTopStakersAsync(int limit = 10)
    {
        try
        {
            var topStakers = await _stakingRepository.GetTopStakersAsync(limit);

            // Add ranking
            for (int i = 0; i < topStakers.Count(); i++)
            {
                topStakers[i].Rank = i + 1;
            }

            return topStakers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top stakers");
            throw;
        }
    }

    public async Task<List<SchoolImpactModel>> GetSchoolImpactStatsAsync()
    {
        try
        {
            // This would be implemented with proper database queries
            // For now, returning mock data structure
            return new List<SchoolImpactModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving school impact stats");
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<decimal> CalculateProjectedMonthlyRewards(IEnumerable<UserStake> activeStakes)
    {
        decimal totalMonthlyRewards = 0;

        foreach (var stake in activeStakes)
        {
            if (stake.StakingPool != null)
            {
                var totalAPY = stake.StakingPool.BaseAPY + stake.StakingPool.BonusAPY;
                var monthlyRate = totalAPY / 12 / 100;
                var monthlyRewards = stake.StakedAmount * monthlyRate * 0.5m; // 50% to user
                totalMonthlyRewards += monthlyRewards;
            }
        }

        return totalMonthlyRewards;
    }

    private async Task<List<UserStakePositionModel>> MapToStakePositionModels(List<UserStake> stakes)
    {
        var positions = new List<UserStakePositionModel>();

        foreach (var stake in stakes)
        {
            var unlockDate = stake.StakeDate.AddDays(stake.StakingPool?.LockPeriodDays ?? 0);
            var daysRemaining = Math.Max(0, (int)(unlockDate - DateTime.UtcNow).TotalDays);
            var canUnstake = DateTime.UtcNow >= unlockDate && stake.IsActive;
            var canClaim = stake.AccruedRewards > 0 && stake.IsActive;

            positions.Add(new UserStakePositionModel
            {
                StakeId = stake.Id,
                PoolId = stake.StakingPoolId,
                PoolName = stake.StakingPool?.Name ?? $"Pool {stake.StakingPoolId}",
                StakedAmount = stake.StakedAmount,
                AccruedRewards = stake.AccruedRewards,
                CurrentAPY = stake.StakingPool != null ? stake.StakingPool.BaseAPY + stake.StakingPool.BonusAPY : 0,
                StakeDate = stake.StakeDate,
                UnlockDate = unlockDate,
                CanUnstake = canUnstake,
                CanClaim = canClaim,
                CanCompound = canClaim,
                DaysRemaining = daysRemaining,
                Status = stake.IsActive ? (canUnstake ? "Unlocked" : "Locked") : "Unstaked",
                StatusClass = stake.IsActive ? (canUnstake ? "unlocked" : "locked") : "unstaked"
            });
        }

        return positions;
    }

    private SchoolBeneficiaryModel MapToSchoolBeneficiaryModel(SchoolBeneficiary school)
    {
        return new SchoolBeneficiaryModel
        {
            Id = school.Id,
            Name = school.Name,
            Description = school.Description,
            Country = school.Country,
            State = school.State,
            City = school.City,
            LogoUrl = school.LogoUrl,
            StudentCount = school.StudentCount,
            TotalReceived = school.TotalReceived,
            IsVerified = school.IsVerified,
            Location = $"{school.City}, {school.State}, {school.Country}".Trim(new[] { ',', ' ' }),
            ImpactSummary = $"Supporting {school.StudentCount} students"
        };
    }

    private List<RewardClaimHistoryModel> MapToRewardHistoryModels(List<StakingRewardClaim> claims)
    {
        return claims.Select(c => new RewardClaimHistoryModel
        {
            ClaimId = c.Id,
            ClaimedAmount = c.ClaimedAmount,
            ClaimDate = c.ClaimDate,
            TransactionHash = c.TransactionHash,
            Status = c.Status.ToString(),
            PoolName = c.UserStake?.StakingPool?.Name ?? "Unknown Pool"
        }).ToList();
    }

    private async Task DistributeSchoolRewardsAsync(string stakerAddress, decimal amount, string transactionHash)
    {
        try
        {
            var selectedSchool = await _stakingRepository.GetUserSelectedSchoolAsync(stakerAddress);
            if (selectedSchool != null)
            {
                var distribution = new SchoolRewardDistribution
                {
                    SchoolBeneficiaryId = selectedSchool.SchoolBeneficiaryId,
                    StakerAddress = stakerAddress,
                    Amount = amount,
                    TransactionHash = transactionHash,
                    Status = TransactionStatus.Confirmed
                };

                await _stakingRepository.CreateSchoolDistributionAsync(distribution);

                // Update school total received
                selectedSchool.SchoolBeneficiary.TotalReceived += amount;

                // Update user total donated
                selectedSchool.TotalDonated += amount;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error distributing school rewards for {StakerAddress}", stakerAddress);
            // Don't fail the main operation if school distribution fails
        }
    }

    private async Task<(decimal TotalFunding, int SchoolsSupported)> GetSchoolFundingStatsAsync()
    {
        // Implementation would query school distributions
        return (0m, 0);
    }

    private decimal CalculateStakingParticipation(decimal totalStaked)
    {
        // Implementation would calculate based on circulating supply
        var circulatingSupply = 1000000000m; // Mock value
        return totalStaked / circulatingSupply * 100;
    }

    private async Task<List<StakingTrendDataModel>> GetStakingTrendsAsync()
    {
        // Implementation would query historical data
        return new List<StakingTrendDataModel>();
    }

    private async Task<List<RewardDistributionDataModel>> GetRewardDistributionAsync()
    {
        // Implementation would query reward distribution data
        return new List<RewardDistributionDataModel>();
    }

    private async Task<List<PoolPerformanceModel>> GetPoolPerformanceAsync()
    {
        var pools = await _stakingRepository.GetActiveStakingPoolsAsync();
        return pools.Select(p => new PoolPerformanceModel
        {
            PoolId = p.Id,
            PoolName = p.Name,
            TotalStaked = p.TotalStaked,
            CurrentAPY = p.BaseAPY + p.BonusAPY,
            Utilization = p.MaxPoolSize > 0 ? (p.TotalStaked / p.MaxPoolSize) * 100 : 0,
            ParticipantCount = 0 // Would be calculated from user stakes
        }).ToList();
    }

    #endregion
}