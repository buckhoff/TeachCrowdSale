using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models.Response;  // ADDED: For Response models
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interface for liquidity repository operations
    /// FIXED: Updated to use Response models and added missing methods
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

        // Analytics - FIXED: All methods now exist and return Response models
        Task<decimal> GetTotalValueLockedAsync();
        Task<decimal> GetTotal24hVolumeAsync();
        Task<decimal> GetTotalFeesGeneratedAsync();
        Task<decimal> GetTotalFeesEarnedAsync();  // ADDED: Alias for consistency
        Task<int> GetActiveLiquidityProvidersCountAsync();
        Task<int> GetActivePoolsCountAsync();  // ADDED: Missing method
        Task<int> GetTotalLiquidityProvidersAsync();  // ADDED: Missing method
        Task<decimal> GetAverageAPYAsync();  // ADDED: Missing method
        Task<List<UserLiquidityPosition>> GetTopLiquidityProvidersAsync(int limit);  // FIXED: Response model
        Task<List<LiquidityPool>> GetPoolPerformanceAsync();  // FIXED: Response model
        Task<List<LiquidityPoolSnapshot>> GetTvlTrendsAsync(int days);  // FIXED: Response model
        Task<List<LiquidityPoolSnapshot>> GetVolumeTrendsAsync(int days);  // FIXED: Response model
    }
}