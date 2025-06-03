using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Infrastructure.Data.Context;

namespace TeachCrowdSale.Infrastructure.Repositories.Staking
{
    /// <summary>
    /// Repository implementation for staking operations
    /// </summary>
    public class StakingRepository : IStakingRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public StakingRepository(TeachCrowdSaleDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Staking Pools

        public async Task<List<StakingPool>> GetActiveStakingPoolsAsync()
        {
            return await _context.StakingPools
                .Where(p => p.IsActive)
                .OrderBy(p => p.LockPeriodDays)
                .ToListAsync();
        }

        public async Task<StakingPool?> GetStakingPoolByIdAsync(int poolId)
        {
            return await _context.StakingPools
                .FirstOrDefaultAsync(p => p.Id == poolId);
        }

        public async Task<StakingPool> UpdateStakingPoolAsync(StakingPool pool)
        {
            pool.UpdatedAt = DateTime.UtcNow;
            _context.StakingPools.Update(pool);
            await _context.SaveChangesAsync();
            return pool;
        }

        #endregion

        #region User Stakes

        public async Task<List<UserStake>> GetUserStakesAsync(string walletAddress, bool activeOnly = false)
        {
            var query = _context.UserStakes
                .Include(s => s.StakingPool)
                .Include(s => s.RewardClaims)
                .Where(s => s.WalletAddress == walletAddress.ToLowerInvariant());

            if (activeOnly)
            {
                query = query.Where(s => s.IsActive);
            }

            return await query
                .OrderByDescending(s => s.StakeDate)
                .ToListAsync();
        }

        public async Task<UserStake?> GetUserStakeByIdAsync(int stakeId)
        {
            return await _context.UserStakes
                .Include(s => s.StakingPool)
                .Include(s => s.RewardClaims)
                .FirstOrDefaultAsync(s => s.Id == stakeId);
        }

        public async Task<UserStake> CreateUserStakeAsync(UserStake stake)
        {
            stake.StakeDate = DateTime.UtcNow;
            stake.LastRewardCalculation = DateTime.UtcNow;
            stake.IsActive = true;

            _context.UserStakes.Add(stake);
            await _context.SaveChangesAsync();
            return stake;
        }

        public async Task<UserStake> UpdateUserStakeAsync(UserStake stake)
        {
            _context.UserStakes.Update(stake);
            await _context.SaveChangesAsync();
            return stake;
        }

        #endregion

        #region Reward Claims

        public async Task<List<StakingRewardClaim>> GetUserRewardClaimsAsync(string walletAddress)
        {
            return await _context.StakingRewardClaims
                .Include(c => c.UserStake)
                .ThenInclude(s => s.StakingPool)
                .Where(c => c.UserStake.WalletAddress == walletAddress.ToLowerInvariant())
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();
        }

        public async Task<StakingRewardClaim> CreateRewardClaimAsync(StakingRewardClaim claim)
        {
            claim.ClaimDate = DateTime.UtcNow;
            claim.Status = TransactionStatus.Pending;

            _context.StakingRewardClaims.Add(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<StakingRewardClaim> UpdateRewardClaimAsync(StakingRewardClaim claim)
        {
            _context.StakingRewardClaims.Update(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        #endregion

        #region School Beneficiaries

        public async Task<List<SchoolBeneficiary>> GetActiveSchoolsAsync(string? country = null, string? state = null)
        {
            var query = _context.SchoolBeneficiaries
                .Where(s => s.IsActive && s.IsVerified);

            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(s => s.Country.ToLower() == country.ToLower());
            }

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(s => s.State.ToLower() == state.ToLower());
            }

            return await query
                .OrderByDescending(s => s.TotalReceived)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<SchoolBeneficiary?> GetSchoolBeneficiaryByIdAsync(int schoolId)
        {
            return await _context.SchoolBeneficiaries
                .FirstOrDefaultAsync(s => s.Id == schoolId && s.IsActive);
        }

        public async Task<UserStakingBeneficiary?> GetUserSelectedSchoolAsync(string walletAddress)
        {
            return await _context.UserStakingBeneficiaries
                .Include(u => u.SchoolBeneficiary)
                .FirstOrDefaultAsync(u => u.WalletAddress == walletAddress.ToLowerInvariant() && u.IsActive);
        }

        public async Task<UserStakingBeneficiary> CreateOrUpdateUserSchoolSelectionAsync(UserStakingBeneficiary selection)
        {
            // Deactivate any existing selections for this wallet
            var existingSelections = await _context.UserStakingBeneficiaries
                .Where(u => u.WalletAddress == selection.WalletAddress.ToLowerInvariant() && u.IsActive)
                .ToListAsync();

            foreach (var existing in existingSelections)
            {
                existing.IsActive = false;
            }

            // Add new selection
            selection.WalletAddress = selection.WalletAddress.ToLowerInvariant();
            selection.SelectedAt = DateTime.UtcNow;
            selection.IsActive = true;

            _context.UserStakingBeneficiaries.Add(selection);
            await _context.SaveChangesAsync();

            return selection;
        }

        #endregion

        #region School Distributions

        public async Task<List<SchoolRewardDistribution>> GetSchoolDistributionsAsync(int schoolId)
        {
            return await _context.SchoolRewardDistributions
                .Where(d => d.SchoolBeneficiaryId == schoolId)
                .OrderByDescending(d => d.DistributionDate)
                .ToListAsync();
        }

        public async Task<SchoolRewardDistribution> CreateSchoolDistributionAsync(SchoolRewardDistribution distribution)
        {
            distribution.DistributionDate = DateTime.UtcNow;
            distribution.Status = TransactionStatus.Pending;

            _context.SchoolRewardDistributions.Add(distribution);
            await _context.SaveChangesAsync();
            return distribution;
        }

        #endregion

        #region Analytics

        public async Task<decimal> GetTotalStakedAsync()
        {
            return await _context.UserStakes
                .Where(s => s.IsActive)
                .SumAsync(s => s.StakedAmount);
        }

        public async Task<decimal> GetTotalRewardsDistributedAsync()
        {
            return await _context.StakingRewardClaims
                .Where(c => c.Status == TransactionStatus.Confirmed)
                .SumAsync(c => c.ClaimedAmount);
        }

        public async Task<int> GetActiveStakersCountAsync()
        {
            return await _context.UserStakes
                .Where(s => s.IsActive)
                .Select(s => s.WalletAddress)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<TopStakerModel>> GetTopStakersAsync(int limit)
        {
            return await _context.UserStakes
                .Where(s => s.IsActive)
                .GroupBy(s => s.WalletAddress)
                .Select(g => new TopStakerModel
                {
                    WalletAddress = g.Key,
                    DisplayAddress = g.Key.Substring(0, 6) + "..." + g.Key.Substring(g.Key.Length - 4),
                    TotalStaked = g.Sum(s => s.StakedAmount),
                    TotalRewards = g.Sum(s => s.ClaimedRewards),
                    FirstStakeDate = g.Min(s => s.StakeDate)
                })
                .OrderByDescending(t => t.TotalStaked)
                .Take(limit)
                .ToListAsync();
        }

        #endregion
    }
}
