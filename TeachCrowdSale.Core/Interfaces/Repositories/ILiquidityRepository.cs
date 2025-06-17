// TeachCrowdSale.Core/Interfaces/Repositories/ILiquidityRepository.cs
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models.Liquidity;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interface for liquidity repository operations
    /// </summary>
    public interface ILiquidityRepository
    {
        // Liquidity Pools
        Task<List<LiquidityPool>> GetActiveLiquidityPoolsAsync();
        Task<LiquidityPool?> GetLiquidityPoolByIdAsync(int poolId);
        Task<LiquidityPool?> GetLiquidityPoolByAddressAsync(string poolAddress);
        Task<LiquidityPool> CreateLiquidityPoolAsync(LiquidityPool pool);
        Task<LiquidityPool> UpdateLiquidityPoolAsync(LiquidityPool pool);
        Task<List<LiquidityPool>> GetPoolsByDexAsync(string dexName);
        Task<List<LiquidityPool>> GetFeaturedPoolsAsync();

        // User Positions
        Task<List<UserLiquidityPosition>> GetUserLiquidityPositionsAsync(string walletAddress, bool activeOnly = false);
        Task<UserLiquidityPosition?> GetUserLiquidityPositionByIdAsync(int positionId);
        Task<UserLiquidityPosition> CreateUserLiquidityPositionAsync(UserLiquidityPosition position);
        Task<UserLiquidityPosition> UpdateUserLiquidityPositionAsync(UserLiquidityPosition position);
        Task<decimal> GetUserTotalLiquidityValueAsync(string walletAddress);

        // Transactions
        Task<List<LiquidityTransaction>> GetUserLiquidityTransactionsAsync(string walletAddress);
        Task<List<LiquidityTransaction>> GetPositionTransactionsAsync(int positionId);
        Task<LiquidityTransaction> CreateLiquidityTransactionAsync(LiquidityTransaction transaction);
        Task<LiquidityTransaction> UpdateLiquidityTransactionAsync(LiquidityTransaction transaction);

        // Pool Snapshots
        Task<List<LiquidityPoolSnapshot>> GetPoolSnapshotsAsync(int poolId, DateTime fromDate, DateTime toDate);
        Task<LiquidityPoolSnapshot?> GetLatestPoolSnapshotAsync(int poolId);
        Task<LiquidityPoolSnapshot> CreatePoolSnapshotAsync(LiquidityPoolSnapshot snapshot);
        Task UpdateLatestSnapshotFlagsAsync(int poolId);

        // DEX Configuration
        Task<List<DexConfiguration>> GetActiveDexConfigurationsAsync();
        Task<DexConfiguration?> GetDexConfigurationByNameAsync(string dexName);
        Task<DexConfiguration> UpdateDexConfigurationAsync(DexConfiguration dexConfig);

        // Analytics
        Task<decimal> GetTotalValueLockedAsync();
        Task<decimal> GetTotal24hVolumeAsync();
        Task<decimal> GetTotalFeesGeneratedAsync();
        Task<int> GetActiveLiquidityProvidersCountAsync();
        Task<List<UserLiquidityStatsModel>> GetTopLiquidityProvidersAsync(int limit);
        Task<List<PoolPerformanceModel>> GetPoolPerformanceDataAsync();
        Task<List<LiquidityTrendDataModel>> GetTvlTrendsAsync(int days);
        Task<List<VolumeTrendDataModel>> GetVolumeTrendsAsync(int days);
    }
}