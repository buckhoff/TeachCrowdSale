// TeachCrowdSale.Core/Interfaces/Services/IDexIntegrationService.cs
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for DEX integration and real-time data fetching
    /// </summary>
    public interface IDexIntegrationService
    {
        // Price and Reserve Data
        Task<decimal> GetTokenPriceAsync(string tokenAddress, string quoteCurrency = "USDC");
        Task<(decimal token0Reserve, decimal token1Reserve, decimal totalSupply)> GetPoolReservesAsync(string poolAddress);
        Task<decimal> GetPoolTotalValueLockedAsync(string poolAddress);
        Task<decimal> GetPool24hVolumeAsync(string poolAddress);

        // APY and Fee Calculations
        Task<decimal> CalculatePoolAPYAsync(string poolAddress, int days = 7);
        Task<decimal> GetPoolFeePercentageAsync(string poolAddress);
        Task<decimal> GetPoolFeesGenerated24hAsync(string poolAddress);

        // Liquidity Calculations
        Task<(decimal token0Amount, decimal token1Amount)> CalculateOptimalLiquidityAmountsAsync(string poolAddress, decimal token0Desired, decimal token1Desired);
        Task<decimal> EstimateLpTokensForAmountsAsync(string poolAddress, decimal token0Amount, decimal token1Amount);
        Task<(decimal token0Amount, decimal token1Amount)> EstimateAmountsForLpTokensAsync(string poolAddress, decimal lpTokenAmount);

        // Price Impact and Slippage
        Task<decimal> CalculatePriceImpactAsync(string poolAddress, decimal token0Amount, decimal token1Amount);
        Task<(decimal token0Min, decimal token1Min)> CalculateMinimumAmountsAsync(string poolAddress, decimal token0Amount, decimal token1Amount, decimal slippageTolerance);

        // Pool Discovery and Validation
        Task<bool> ValidatePoolExistsAsync(string poolAddress);
        Task<string?> FindPoolAddressAsync(string token0Address, string token1Address, string dexName);
        Task<List<string>> GetAllPoolsForTokenAsync(string tokenAddress, string dexName);

        Task<decimal> GetTotalLpTokenSupplyAsync(string poolAddress);

        // Transaction Simulation
        Task<bool> SimulateAddLiquidityAsync(string walletAddress, string poolAddress, decimal token0Amount, decimal token1Amount);
        Task<bool> SimulateRemoveLiquidityAsync(string walletAddress, string poolAddress, decimal lpTokenAmount);
    }
}