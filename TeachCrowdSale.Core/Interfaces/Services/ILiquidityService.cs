// TeachCrowdSale.Core/Interfaces/Services/ILiquidityService.cs
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models.Response;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for liquidity service operations
    /// ARCHITECTURE FIX: Now returns only Response models (not web Models)
    /// </summary>
    public interface ILiquidityService
    {
        #region Pool Management

        /// <summary>
        /// Get active liquidity pools
        /// </summary>
        Task<List<LiquidityPoolResponse>> GetActiveLiquidityPoolsAsync();

        /// <summary>
        /// Get specific liquidity pool by ID
        /// </summary>
        Task<LiquidityPoolResponse?> GetLiquidityPoolAsync(int poolId);

        /// <summary>
        /// Get liquidity statistics overview
        /// </summary>
        Task<LiquidityStatsResponse> GetLiquidityStatsAsync();

        /// <summary>
        /// Get DEX configuration options
        /// </summary>
        Task<List<DexConfigurationResponse>> GetDexConfigurationsAsync();

        #endregion

        #region User Position Management

        /// <summary>
        /// Get user's liquidity positions
        /// </summary>
        Task<List<UserLiquidityPositionResponse>> GetUserLiquidityPositionsAsync(string walletAddress);

        /// <summary>
        /// Get specific user liquidity position
        /// </summary>
        Task<UserLiquidityPositionResponse?> GetUserLiquidityPositionAsync(int positionId);

        /// <summary>
        /// Get user's total liquidity value
        /// </summary>
        Task<decimal> GetUserTotalLiquidityValueAsync(string walletAddress);

        /// <summary>
        /// Get comprehensive user liquidity information
        /// </summary>
        Task<UserLiquidityInfoResponse> GetUserLiquidityInfoAsync(string walletAddress);

        #endregion

        #region Liquidity Calculations

        /// <summary>
        /// Calculate liquidity addition preview
        /// </summary>
        Task<LiquidityCalculationResponse> CalculateLiquidityPreviewAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal? token1Amount = null,
            decimal slippageTolerance = 0.5m);

        /// <summary>
        /// Calculate liquidity removal preview
        /// </summary>
        Task<LiquidityCalculationResponse> CalculateRemoveLiquidityPreviewAsync(
            string walletAddress,
            int positionId,
            decimal percentageToRemove);

        #endregion

        #region Liquidity Operations

        /// <summary>
        /// Add liquidity to pool
        /// </summary>
        Task<bool> AddLiquidityAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal token1Amount,
            decimal token0AmountMin,
            decimal token1AmountMin);

        /// <summary>
        /// Remove liquidity from position
        /// </summary>
        Task<bool> RemoveLiquidityAsync(
            string walletAddress,
            int positionId,
            decimal percentageToRemove);

        /// <summary>
        /// Claim accumulated fees from position
        /// </summary>
        Task<bool> ClaimFeesAsync(string walletAddress, int positionId);

        #endregion

        #region Pool Data Synchronization

        /// <summary>
        /// Sync specific pool data from DEX
        /// </summary>
        Task SyncPoolDataAsync(int poolId);

        /// <summary>
        /// Sync all pools data from DEX
        /// </summary>
        Task SyncAllPoolsDataAsync();

        /// <summary>
        /// Refresh pool prices from DEX APIs
        /// </summary>
        Task<bool> RefreshPoolPricesAsync();

        #endregion

        #region Analytics

        /// <summary>
        /// Get liquidity analytics data
        /// </summary>
        Task<LiquidityAnalyticsResponse> GetLiquidityAnalyticsAsync();

        /// <summary>
        /// Get top liquidity providers
        /// </summary>
        Task<List<UserLiquidityStatsResponse>> GetTopLiquidityProvidersAsync(int limit = 10);

        /// <summary>
        /// Get pool performance data
        /// </summary>
        Task<List<PoolPerformanceDataResponse>> GetPoolPerformanceAsync();

        /// <summary>
        /// Get TVL trends over time
        /// </summary>
        Task<List<LiquidityTrendDataResponse>> GetTvlTrendsAsync(int days = 30);

        /// <summary>
        /// Get volume trends over time
        /// </summary>
        Task<List<VolumeTrendDataResponse>> GetVolumeTrendsAsync(int days = 30);

        #endregion

        #region DEX Integration

        /// <summary>
        /// Get token price from DEX
        /// </summary>
        Task<decimal> GetTokenPriceFromDexAsync(string tokenAddress, string dexName);

        /// <summary>
        /// Get pool reserves from DEX
        /// </summary>
        Task<(decimal token0Reserve, decimal token1Reserve)> GetPoolReservesAsync(string poolAddress, string dexName);

        /// <summary>
        /// Get pool APY from DEX
        /// </summary>
        Task<decimal> GetPoolAPYAsync(int poolId);

        #endregion

        #region Guidance and Education

        /// <summary>
        /// Get liquidity guide steps for user
        /// </summary>
        Task<List<LiquidityGuideStepResponse>> GetLiquidityGuideStepsAsync(string? walletAddress = null);

        /// <summary>
        /// Get comprehensive liquidity page data
        /// </summary>
        Task<LiquidityPageDataResponse> GetLiquidityPageDataAsync();

        #endregion

        #region Validation

        /// <summary>
        /// Validate liquidity parameters
        /// </summary>
        Task<LiquidityValidationResponse> ValidateLiquidityParametersAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal token1Amount);

        #endregion
    }
}