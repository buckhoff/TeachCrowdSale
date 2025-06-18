// TeachCrowdSale.Core/Interfaces/Services/ILiquidityService.cs
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Data.Enum;
using TeachCrowdSale.Core.Models.Response;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for liquidity service operations
    /// FIXED: Added missing method signatures to match implementation
    /// </summary>
    public interface ILiquidityService
    {
        #region Pool Management
        Task<List<LiquidityPoolResponse>> GetActiveLiquidityPoolsAsync();
        Task<LiquidityPool?> GetLiquidityPoolAsync(int poolId);
        Task<LiquidityStatsResponse> GetLiquidityStatsAsync();
        Task<List<DexConfigurationResponse>> GetDexConfigurationsAsync();

        // ADDED: Comprehensive page data method
        Task<LiquidityPageDataResponse> GetLiquidityPageDataAsync();
        #endregion

        #region User Position Management
        Task<List<UserLiquidityPositionResponse>> GetUserLiquidityPositionsAsync(string walletAddress);
        Task<UserLiquidityPositionResponse?> GetUserLiquidityPositionAsync(int positionId);
        Task<decimal> GetUserTotalLiquidityValueAsync(string walletAddress);
        Task<UserLiquidityInfoResponse> GetUserLiquidityInfoAsync(string walletAddress);

        // ADDED: User transaction history method
        Task<List<LiquidityTransactionHistoryResponse>> GetUserTransactionHistoryAsync(string walletAddress, int page = 1, int pageSize = 10);
        #endregion

        Task<List<LiquidityTrendDataResponse>> GetTvlTrendsAsync(int days = 30);
        Task<List<VolumeTrendDataResponse>> GetVolumeTrendsAsync(int days = 30);

        #region Liquidity Calculations
        Task<LiquidityCalculationResponse> CalculateLiquidityPreviewAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal? token1Amount = null,
            decimal slippageTolerance = 0.5m);

        Task<LiquidityCalculationResponse> CalculateRemoveLiquidityPreviewAsync(
            string walletAddress,
            int positionId,
            decimal percentageToRemove);
        #endregion

        #region Liquidity Operations
        Task<bool> AddLiquidityAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal token1Amount,
            decimal token0AmountMin,
            decimal token1AmountMin);

        Task<bool> RemoveLiquidityAsync(
            string walletAddress,
            int positionId,
            decimal percentageToRemove);

        Task<bool> ClaimFeesAsync(string walletAddress, int positionId);
        #endregion

        #region Pool Data Synchronization
        Task SyncPoolDataAsync(int poolId);
        Task SyncAllPoolsDataAsync();
        Task<bool> RefreshPoolPricesAsync();
        #endregion

        #region Analytics
        Task<LiquidityAnalyticsResponse> GetLiquidityAnalyticsAsync();
        Task<List<UserLiquidityStatsResponse>> GetTopLiquidityProvidersAsync(int limit = 10);
        Task<List<PoolPerformanceDataResponse>> GetPoolPerformanceAsync();

        // ADDED: DEX comparison method that returns Response model
        Task<List<DexPerformanceResponse>> GetDexComparisonDataAsync();
        #endregion

        #region Validation & Transaction Operations
        /// <summary>
        /// Validate liquidity parameters for add liquidity operation
        /// Called by API Controller's ValidateLiquidityParameters endpoint
        /// </summary>
        Task<LiquidityValidationResponse> ValidateLiquidityParametersAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal token1Amount);

        /// <summary>
        /// Comprehensive transaction validation for any liquidity operation
        /// More flexible method for complex validation scenarios
        /// </summary>
        Task<LiquidityValidationResponse> ValidateTransactionAsync(
            string walletAddress,
            string transactionType,
            int? poolId = null,
            int? positionId = null,
            decimal? token0Amount = null,
            decimal? token1Amount = null,
            decimal? percentageToRemove = null);

        /// <summary>
        /// Validate remove liquidity transaction
        /// </summary>
        Task<LiquidityValidationResponse> ValidateRemoveLiquidityAsync(
            string walletAddress,
            int positionId,
            decimal percentageToRemove);
        #endregion
        
        #region Guide System
        Task<List<LiquidityGuideStepResponse>> GetLiquidityGuideStepsAsync(string dexName);
        Task<bool> MarkGuideStepCompletedAsync(string walletAddress, int stepNumber);
        #endregion
    }
}