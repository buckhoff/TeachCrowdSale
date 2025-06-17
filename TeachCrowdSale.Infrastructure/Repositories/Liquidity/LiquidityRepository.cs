// TeachCrowdSale.Infrastructure/Repositories/Liquidity/LiquidityRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Infrastructure.Data.Context;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Infrastructure.Repositories.Liquidity
{
    /// <summary>
    /// Repository implementation for liquidity operations
    /// </summary>
    public class LiquidityRepository : ILiquidityRepository
    {
        private readonly TeachCrowdSaleDbContext _context;
        private readonly ILogger<LiquidityRepository> _logger;

        public LiquidityRepository(
            TeachCrowdSaleDbContext context,
            ILogger<LiquidityRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Liquidity Pools

        public async Task<List<LiquidityPool>> GetActiveLiquidityPoolsAsync()
        {
            return await _context.LiquidityPools
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.TotalValueLocked)
                .ToListAsync();
        }

        public async Task<LiquidityPool?> GetLiquidityPoolByIdAsync(int poolId)
        {
            return await _context.LiquidityPools
                .Include(p => p.UserPositions.Where(up => up.IsActive))
                .Include(p => p.Snapshots.Where(s => s.IsLatest))
                .FirstOrDefaultAsync(p => p.Id == poolId);
        }

        public async Task<LiquidityPool?> GetLiquidityPoolByAddressAsync(string poolAddress)
        {
            return await _context.LiquidityPools
                .FirstOrDefaultAsync(p => p.PoolAddress.ToLower() == poolAddress.ToLower());
        }

        public async Task<LiquidityPool> CreateLiquidityPoolAsync(LiquidityPool pool)
        {
            pool.CreatedAt = DateTime.UtcNow;
            pool.UpdatedAt = DateTime.UtcNow;
            pool.LastSyncAt = DateTime.UtcNow;

            _context.LiquidityPools.Add(pool);
            await _context.SaveChangesAsync();
            return pool;
        }

        public async Task<LiquidityPool> UpdateLiquidityPoolAsync(LiquidityPool pool)
        {
            pool.UpdatedAt = DateTime.UtcNow;
            pool.LastSyncAt = DateTime.UtcNow;

            _context.LiquidityPools.Update(pool);
            await _context.SaveChangesAsync();
            return pool;
        }

        public async Task<List<LiquidityPool>> GetPoolsByDexAsync(string dexName)
        {
            return await _context.LiquidityPools
                .Where(p => p.IsActive && p.DexName.ToLower() == dexName.ToLower())
                .OrderByDescending(p => p.TotalValueLocked)
                .ToListAsync();
        }

        public async Task<List<LiquidityPool>> GetFeaturedPoolsAsync()
        {
            return await _context.LiquidityPools
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.TotalValueLocked)
                .ToListAsync();
        }

        #endregion

        #region User Positions

        public async Task<List<UserLiquidityPosition>> GetUserLiquidityPositionsAsync(string walletAddress, bool activeOnly = false)
        {
            var query = _context.UserLiquidityPositions
                .Include(p => p.LiquidityPool)
                .Include(p => p.Transactions)
                .Where(p => p.WalletAddress == walletAddress.ToLowerInvariant());

            if (activeOnly)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query
                .OrderByDescending(p => p.AddedAt)
                .ToListAsync();
        }

        public async Task<UserLiquidityPosition?> GetUserLiquidityPositionByIdAsync(int positionId)
        {
            return await _context.UserLiquidityPositions
                .Include(p => p.LiquidityPool)
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == positionId);
        }

        public async Task<UserLiquidityPosition> CreateUserLiquidityPositionAsync(UserLiquidityPosition position)
        {
            position.WalletAddress = position.WalletAddress.ToLowerInvariant();
            position.AddedAt = DateTime.UtcNow;
            position.LastUpdatedAt = DateTime.UtcNow;
            position.IsActive = true;

            _context.UserLiquidityPositions.Add(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<UserLiquidityPosition> UpdateUserLiquidityPositionAsync(UserLiquidityPosition position)
        {
            position.LastUpdatedAt = DateTime.UtcNow;

            _context.UserLiquidityPositions.Update(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<decimal> GetUserTotalLiquidityValueAsync(string walletAddress)
        {
            return await _context.UserLiquidityPositions
                .Where(p => p.WalletAddress == walletAddress.ToLowerInvariant() && p.IsActive)
                .SumAsync(p => p.CurrentValueUsd);
        }

        #endregion

        #region Transactions

        public async Task<List<LiquidityTransaction>> GetUserLiquidityTransactionsAsync(string walletAddress)
        {
            return await _context.LiquidityTransactions
                .Include(t => t.UserLiquidityPosition)
                .ThenInclude(p => p.LiquidityPool)
                .Where(t => t.WalletAddress == walletAddress.ToLowerInvariant())
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }

        public async Task<List<LiquidityTransaction>> GetPositionTransactionsAsync(int positionId)
        {
            return await _context.LiquidityTransactions
                .Where(t => t.UserLiquidityPositionId == positionId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }

        public async Task<LiquidityTransaction> CreateLiquidityTransactionAsync(LiquidityTransaction transaction)
        {
            transaction.WalletAddress = transaction.WalletAddress.ToLowerInvariant();
            transaction.Timestamp = DateTime.UtcNow;

            _context.LiquidityTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<LiquidityTransaction> UpdateLiquidityTransactionAsync(LiquidityTransaction transaction)
        {
            _context.LiquidityTransactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        #endregion

        #region Pool Snapshots

        public async Task<List<LiquidityPoolSnapshot>> GetPoolSnapshotsAsync(int poolId, DateTime fromDate, DateTime toDate)
        {
            return await _context.LiquidityPoolSnapshots
                .Where(s => s.LiquidityPoolId == poolId &&
                           s.Timestamp >= fromDate &&
                           s.Timestamp <= toDate)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<LiquidityPoolSnapshot?> GetLatestPoolSnapshotAsync(int poolId)
        {
            return await _context.LiquidityPoolSnapshots
                .Where(s => s.LiquidityPoolId == poolId && s.IsLatest)
                .FirstOrDefaultAsync();
        }

        public async Task<LiquidityPoolSnapshot> CreatePoolSnapshotAsync(LiquidityPoolSnapshot snapshot)
        {
            snapshot.Timestamp = DateTime.UtcNow;

            _context.LiquidityPoolSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
            return snapshot;
        }

        public async Task UpdateLatestSnapshotFlagsAsync(int poolId)
        {
            // Reset all latest flags for this pool
            await _context.LiquidityPoolSnapshots
                .Where(s => s.LiquidityPoolId == poolId && s.IsLatest)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsLatest, false));

            // Set the most recent snapshot as latest
            var latestSnapshot = await _context.LiquidityPoolSnapshots
                .Where(s => s.LiquidityPoolId == poolId)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();

            if (latestSnapshot != null)
            {
                latestSnapshot.IsLatest = true;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region DEX Configuration

        public async Task<List<DexConfiguration>> GetActiveDexConfigurationsAsync()
        {
            return await _context.DexConfigurations
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<DexConfiguration?> GetDexConfigurationByNameAsync(string dexName)
        {
            return await _context.DexConfigurations
                .FirstOrDefaultAsync(d => d.Name.ToLower() == dexName.ToLower() && d.IsActive);
        }

        public async Task<DexConfiguration> UpdateDexConfigurationAsync(DexConfiguration dexConfig)
        {
            dexConfig.UpdatedAt = DateTime.UtcNow;

            _context.DexConfigurations.Update(dexConfig);
            await _context.SaveChangesAsync();
            return dexConfig;
        }

        #endregion

        #region Analytics

        public async Task<decimal> GetTotalValueLockedAsync()
        {
            return await _context.LiquidityPools
                .Where(p => p.IsActive)
                .SumAsync(p => p.TotalValueLocked);
        }

        public async Task<decimal> GetTotal24hVolumeAsync()
        {
            return await _context.LiquidityPools
                .Where(p => p.IsActive)
                .SumAsync(p => p.Volume24h);
        }

        public async Task<decimal> GetTotalFeesGeneratedAsync()
        {
            return await _context.UserLiquidityPositions
                .Where(p => p.IsActive)
                .SumAsync(p => p.FeesEarnedUsd);
        }

        public async Task<int> GetActiveLiquidityProvidersCountAsync()
        {
            return await _context.UserLiquidityPositions
                .Where(p => p.IsActive)
                .Select(p => p.WalletAddress)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<UserLiquidityStatsModel>> GetTopLiquidityProvidersAsync(int limit)
        {
            return await _context.UserLiquidityPositions
                .Where(p => p.IsActive)
                .GroupBy(p => p.WalletAddress)
                .Select(g => new UserLiquidityStatsModel
                {
                    WalletAddress = g.Key,
                    DisplayAddress = g.Key.Substring(0, 6) + "..." + g.Key.Substring(g.Key.Length - 4),
                    TotalValueProvided = g.Sum(p => p.CurrentValueUsd),
                    TotalFeesEarned = g.Sum(p => p.FeesEarnedUsd),
                    ActivePositions = g.Count(),
                    FirstProvisionDate = g.Min(p => p.AddedAt)
                })
                .OrderByDescending(u => u.TotalValueProvided)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<PoolPerformanceModel>> GetPoolPerformanceDataAsync()
        {
            return await _context.LiquidityPools
                .Where(p => p.IsActive)
                .Select(p => new PoolPerformanceModel
                {
                    PoolId = p.Id,
                    PoolName = p.Name,
                    TotalValueLocked = p.TotalValueLocked,
                    Volume24h = p.Volume24h,
                    APY = p.APY,
                    FeesGenerated = p.UserPositions.Where(up => up.IsActive).Sum(up => up.FeesEarnedUsd),
                    ProvidersCount = p.UserPositions.Where(up => up.IsActive).Select(up => up.WalletAddress).Distinct().Count()
                })
                .OrderByDescending(p => p.TotalValueLocked)
                .ToListAsync();
        }

        public async Task<List<LiquidityTrendDataModel>> GetTvlTrendsAsync(int days)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            return await _context.LiquidityPoolSnapshots
                .Where(s => s.Timestamp >= fromDate)
                .GroupBy(s => s.Timestamp.Date)
                .Select(g => new LiquidityTrendDataModel
                {
                    Date = g.Key,
                    TotalValueLocked = g.Sum(s => s.TotalValueLocked),
                    Volume = g.Sum(s => s.Volume24h),
                    APY = g.Average(s => s.APY)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();
        }

        public async Task<List<VolumeTrendDataModel>> GetVolumeTrendsAsync(int days)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            return await _context.LiquidityPoolSnapshots
                .Where(s => s.Timestamp >= fromDate)
                .GroupBy(s => s.Timestamp.Date)
                .Select(g => new VolumeTrendDataModel
                {
                    Date = g.Key,
                    Volume24h = g.Sum(s => s.Volume24h),
                    FeesGenerated = g.Sum(s => s.Volume24h * 0.003m), // Estimate 0.3% fee
                    TransactionCount = 0 // Would need separate transaction tracking
                })
                .OrderBy(t => t.Date)
                .ToListAsync();
        }

        #endregion
    }
}