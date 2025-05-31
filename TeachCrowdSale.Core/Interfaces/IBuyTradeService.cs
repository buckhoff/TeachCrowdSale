using TeachCrowdSale.Core.Models.Request;
using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Interfaces;

/// <summary>
/// Service interface for buy/trade page operations
/// </summary>
public interface IBuyTradeService
{
    /// <summary>
    /// Get comprehensive buy/trade page data
    /// </summary>
    Task<BuyTradeDataModel> GetBuyTradeDataAsync();

    /// <summary>
    /// Calculate purchase price for given parameters
    /// </summary>
    Task<PriceCalculationModel> CalculatePriceAsync(string address, int tierId, decimal usdAmount);

    /// <summary>
    /// Get user's trade information and purchase history
    /// </summary>
    Task<UserTradeInfoModel> GetUserTradeInfoAsync(string address);

    /// <summary>
    /// Validate a purchase request
    /// </summary>
    Task<PurchaseValidationModel> ValidatePurchaseAsync(string address, int tierId, decimal amount);

    /// <summary>
    /// Get wallet information including balances and user data
    /// </summary>
    Task<WalletInfoModel> GetWalletInfoAsync(string address);

    /// <summary>
    /// Get DEX trading information
    /// </summary>
    Task<DexInfoModel> GetDexInfoAsync();

    /// <summary>
    /// Get fallback data when API calls fail
    /// </summary>
    BuyTradeDataModel GetFallbackBuyTradeData();

    /// <summary>
    /// Check service health status
    /// </summary>
    Task<bool> CheckServiceHealthAsync();
}