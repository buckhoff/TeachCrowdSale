using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Web service interface for liquidity dashboard operations
    /// Follows established TeachToken patterns for dashboard services
    /// Maps Response models from API to Web Models for UI consumption
    /// </summary>
    public interface ILiquidityDashboardService
    {
        #region Dashboard Overview

        /// <summary>
        /// Get comprehensive liquidity dashboard data
        /// Maps from LiquidityPageDataResponse to LiquidityPageDataModel
        /// </summary>
        Task<LiquidityPageDataModel?> GetLiquidityPageDataAsync();

        /// <summary>
        /// Get liquidity statistics overview
        /// Maps from LiquidityStatsResponse to LiquidityStatsModel
        /// </summary>
        Task<LiquidityStatsModel?> GetLiquidityStatsAsync();

        /// <summary>
        /// Get liquidity analytics data
        /// Maps from LiquidityAnalyticsResponse to LiquidityAnalyticsModel
        /// </summary>
        Task<LiquidityAnalyticsModel?> GetLiquidityAnalyticsAsync();

        #endregion

        #region Pool Management

        /// <summary>
        /// Get active liquidity pools
        /// Maps from List<LiquidityPoolResponse> to List<LiquidityPoolModel>
        /// </summary>
        Task<List<LiquidityPoolModel>?> GetActiveLiquidityPoolsAsync();

        /// <summary>
        /// Get specific pool details
        /// Maps from LiquidityPoolResponse to LiquidityPoolModel
        /// </summary>
        Task<LiquidityPoolModel?> GetLiquidityPoolDetailsAsync(int poolId);

        /// <summary>
        /// Get DEX configuration options
        /// Maps from List<DexConfigurationResponse> to List<DexConfigurationModel>
        /// </summary>
        Task<List<DexConfigurationModel>?> GetDexConfigurationsAsync();

        #endregion

        #region User-Specific Data

        /// <summary>
        /// Get user's liquidity positions
        /// Maps from List<UserLiquidityPositionResponse> to List<UserLiquidityPositionModel>
        /// </summary>
        Task<List<UserLiquidityPositionModel>?> GetUserLiquidityPositionsAsync(string walletAddress);

        /// <summary>
        /// Get user's comprehensive liquidity information
        /// Maps from UserLiquidityInfoResponse to UserLiquidityInfoModel (note: currently in wrong namespace)
        /// </summary>
        Task<UserLiquidityInfoModel?> GetUserLiquidityInfoAsync(string walletAddress);

        /// <summary>
        /// Get user's transaction history
        /// Maps from List<LiquidityTransactionHistoryResponse> to List<LiquidityTransactionHistoryModel>
        /// </summary>
        Task<List<LiquidityTransactionHistoryModel>?> GetUserTransactionHistoryAsync(string walletAddress, int page = 1, int pageSize = 20);

        #endregion

        #region Calculations and Previews

        /// <summary>
        /// Calculate liquidity addition preview
        /// Maps from LiquidityCalculationResponse to LiquidityCalculatorModel
        /// </summary>
        Task<LiquidityCalculatorModel?> CalculateAddLiquidityPreviewAsync(
            string walletAddress,
            int poolId,
            decimal token0Amount,
            decimal? token1Amount = null,
            decimal slippageTolerance = 0.5m);

        /// <summary>
        /// Calculate liquidity removal preview
        /// Maps from LiquidityCalculationResponse to LiquidityCalculatorModel
        /// </summary>
        Task<LiquidityCalculatorModel?> CalculateRemoveLiquidityPreviewAsync(
            string walletAddress,
            int positionId,
            decimal percentageToRemove);

        #endregion

        #region Add Liquidity Wizard Support

        /// <summary>
        /// Get wizard step data for add liquidity flow
        /// Combines multiple API calls to populate AddLiquidityModel
        /// </summary>
        Task<AddLiquidityModel?> GetAddLiquidityWizardDataAsync(string? walletAddress = null);

        /// <summary>
        /// Get liquidity guide steps for specific DEX
        /// Maps from List<LiquidityGuideStepResponse> to List<LiquidityGuideStepModel>
        /// </summary>
        Task<List<LiquidityGuideStepModel>?> GetLiquidityGuideStepsAsync(string? dexName = null);

        #endregion

        #region Cache Management

        /// <summary>
        /// Clear user-specific cache data
        /// </summary>
        void ClearUserCache(string walletAddress);

        /// <summary>
        /// Clear all cached data
        /// </summary>
        void ClearAllCache();

        #endregion
    }
}