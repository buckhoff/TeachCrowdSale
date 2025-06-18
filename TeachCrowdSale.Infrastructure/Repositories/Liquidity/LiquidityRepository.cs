// TeachCrowdSale.Infrastructure/Repositories/Liquidity/LiquidityRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Models.Response;
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

        public async Task<List<UserLiquidityStatsResponse>> GetTopLiquidityProvidersAsync(int limit)
        {
            var userStats = await _context.UserLiquidityPositions
                .Where(p => p.IsActive)
                .GroupBy(p => p.WalletAddress)
                .Select(g => new UserLiquidityStatsResponse
                {
                    WalletAddress = g.Key,
                    DisplayAddress = g.Key.Substring(0, 6) + "..." + g.Key.Substring(g.Key.Length - 4),
                    TotalLiquidityValue = g.Sum(p => p.CurrentValueUsd),
                    TotalValueProvided = g.Sum(p => p.InitialValueUsd),  // FIXED: Added missing property
                    TotalFeesEarned = g.Sum(p => p.FeesEarnedUsd),
                    TotalPnL = g.Sum(p => p.NetPnL),
                    PnLPercentage = g.Sum(p => p.InitialValueUsd) > 0 ?
                        (g.Sum(p => p.NetPnL) / g.Sum(p => p.InitialValueUsd)) * 100 : 0,
                    ActivePositions = g.Count(),
                    FirstPositionDate = g.Min(p => p.AddedAt),
                    TimeActive = DateTime.UtcNow - g.Min(p => p.AddedAt)
                })
                .OrderByDescending(u => u.TotalLiquidityValue)
                .Take(limit)
                .ToListAsync();

            // Add ranking
            for (int i = 0; i < userStats.Count; i++)
            {
                userStats[i].Rank = i + 1;
            }

            return userStats;
        }

        public async Task<List<PoolPerformanceDataResponse>> GetPoolPerformanceDataAsync()
        {
            return await _context.LiquidityPools
                .Where(p => p.IsActive)
                .Select(p => new PoolPerformanceDataResponse
                {
                    PoolId = p.Id,
                    PoolName = p.Name,
                    TokenPair = p.TokenPair,
                    DexName = p.DexName,
                    APY = p.APY,
                    TotalValueLocked = p.TotalValueLocked,
                    Volume24h = p.Volume24h,
                    FeesGenerated24h = p.Volume24h * (p.FeePercentage / 100),  // Calculate fees
                    PriceChange24h = 0, // Would need historical data to calculate
                    ProvidersCount = p.UserPositions.Where(up => up.IsActive).Select(up => up.WalletAddress).Distinct().Count(),
                    LastUpdated = p.UpdatedAt
                })
                .OrderByDescending(p => p.TotalValueLocked)
                .ToListAsync();
        }

        public async Task<List<LiquidityTrendDataResponse>> GetTvlTrendsAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            return await _context.LiquidityPoolSnapshots
                .Where(s => s.Timestamp >= startDate)
                .GroupBy(s => s.Timestamp.Date)
                .Select(g => new LiquidityTrendDataResponse
                {
                    Date = g.Key,
                    TotalValueLocked = g.Sum(s => s.TotalValueLocked),
                    Change24h = 0, // Would need previous day's data to calculate
                    ChangePercentage = 0
                })
                .OrderBy(t => t.Date)
                .ToListAsync();
        }

        public async Task<List<VolumeTrendDataResponse>> GetVolumeTrendsAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            return await _context.LiquidityPoolSnapshots
                .Where(s => s.Timestamp >= startDate)
                .GroupBy(s => s.Timestamp.Date)
                .Select(g => new VolumeTrendDataResponse
                {
                    Date = g.Key,
                    Volume = g.Sum(s => s.Volume24h),
                    Change24h = 0, // Would need previous day's data to calculate
                    ChangePercentage = 0,
                    TransactionCount = 0 // Would need transaction data to calculate
                })
                .OrderBy(t => t.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Get count of active liquidity pools
        /// </summary>
        public async Task<int> GetActivePoolsCountAsync()
        {
            return await _context.LiquidityPools
                .Where(p => p.IsActive)
                .CountAsync();
        }

        /// <summary>
        /// Get total count of unique liquidity providers
        /// </summary>
        public async Task<int> GetTotalLiquidityProvidersAsync()
        {
            return await _context.UserLiquidityPositions
                .Where(p => p.IsActive)
                .Select(p => p.WalletAddress)
                .Distinct()
                .CountAsync();
        }

        /// <summary>
        /// Get average APY across all active pools
        /// </summary>
        public async Task<decimal> GetAverageAPYAsync()
        {
            var activePools = await _context.LiquidityPools
                .Where(p => p.IsActive)
                .Select(p => p.APY)
                .ToListAsync();

            return activePools.Any() ? activePools.Average() : 0;
        }

        /// <summary>
        /// Get total fees earned by all users (alias for GetTotalFeesGeneratedAsync)
        /// </summary>
        public async Task<decimal> GetTotalFeesEarnedAsync()
        {
            return await GetTotalFeesGeneratedAsync();
        }

        #endregion
    }
}