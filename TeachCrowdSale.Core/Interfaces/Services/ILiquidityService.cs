// TeachCrowdSale.Core/Interfaces/Services/ILiquidityService.cs
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models;
using TeachCrowdSale.Core.Models.Liquidity;
using TeachCrowdSale.Core.Models.Response;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for liquidity service operations
    /// </summary>
    public interface ILiquidityService
    {
        // Pool Management
        Task<List<LiquidityPoolDisplayModel>> GetActiveLiquidityPoolsAsync();
        Task<LiquidityPool?> GetLiquidityPoolAsync(int poolId);
        Task<LiquidityStatsOverviewModel> GetLiquidityStatsAsync();
        Task<List<DexConfigurationModel>> GetDexConfigurationsAsync();

        // User Position Management
        Task<List<UserLiquidityPositionModel>> GetUserLiquidityPositionsAsync(string walletAddress);
        Task<UserLiquidityPositionModel?> GetUserLiquidityPositionAsync(int positionId);
        Task<decimal> GetUserTotalLiquidityValueAsync(string walletAddress);

        // Liquidity Calculations
        Task<LiquidityCalculationModel> CalculateLiquidityPreviewAsync(string walletAddress, int poolId, decimal token0Amount, decimal? token1Amount = null, decimal slippageTolerance = 0.5m);
        Task<LiquidityCalculationModel> CalculateRemoveLiquidityPreviewAsync(string walletAddress, int positionId, decimal percentageToRemove);

        // Liquidity Operations
        Task<bool> AddLiquidityAsync(string walletAddress, int poolId, decimal token0Amount, decimal token1Amount, decimal token0AmountMin, decimal token1AmountMin);
        Task<bool> RemoveLiquidityAsync(string walletAddress, int positionId, decimal percentageToRemove);
        Task<bool> ClaimFeesAsync(string walletAddress, int positionId);

        // Pool Data Synchronization
        Task SyncPoolDataAsync(int poolId);
        Task SyncAllPoolsDataAsync();
        Task<bool> RefreshPoolPricesAsync();

        // Analytics
        Task<LiquidityAnalyticsModel> GetLiquidityAnalyticsAsync();
        Task<List<UserLiquidityStatsModel>> GetTopLiquidityProvidersAsync(int limit = 10);
        Task<List<PoolPerformanceDataModel>> GetPoolPerformanceAsync();
        Task<List<LiquidityTrendDataModel>> GetTvlTrendsAsync(int days = 30);
        Task<List<VolumeTrendDataModel>> GetVolumeTrendsAsync(int days = 30);

        // DEX Integration
        Task<decimal> GetTokenPriceFromDexAsync(string tokenAddress, string dexName);
        Task<(decimal token0Reserve, decimal token1Reserve)> GetPoolReservesAsync(string poolAddress, string dexName);
        Task<decimal> GetPoolAPYAsync(int poolId);

        // Guidance and Education
        Task<List<LiquidityGuideStepModel>> GetLiquidityGuideStepsAsync(string? walletAddress = null);
        Task<bool> MarkGuideStepCompletedAsync(string walletAddress, int stepNumber);
        Task<UserLiquidityInfoModel> GetUserLiquidityInfoAsync(string walletAddress);
        Task<LiquidityValidationResponse> ValidateTransactionAsync(string walletAddress, string transactionType, int? poolId = null, int? positionId = null, decimal? token0Amount = null, decimal? token1Amount = null, decimal? percentageToRemove = null);
        Task<List<RewardProjectionModel>> GetLiquidityRewardProjectionsAsync(string walletAddress, int poolId, decimal token0Amount, decimal token1Amount, int days = 365);
        Task<List<LiquidityTransactionHistoryModel>> GetUserTransactionHistoryAsync(string walletAddress, int pageNumber = 1, int pageSize = 50);
        Task<bool> UpdateUserPositionValuesAsync(string walletAddress);
        Task<decimal> EstimateTransactionGasAsync(string walletAddress, string transactionType, int? poolId = null, int? positionId = null);
    }
}