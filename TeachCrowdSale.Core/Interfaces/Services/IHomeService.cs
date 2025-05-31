using TeachCrowdSale.Core.Models.Response;

namespace TeachCrowdSale.Core.Interfaces.Services;

/// <summary>
/// Service interface for home page data operations
/// </summary>
public interface IHomeService
{
    /// <summary>
    /// Get comprehensive home page data
    /// </summary>
    Task<HomePageDataModel> GetHomePageDataAsync();

    /// <summary>
    /// Get live statistics for real-time updates
    /// </summary>
    Task<LiveStatsModel> GetLiveStatsAsync();

    /// <summary>
    /// Get all tier information for display
    /// </summary>
    Task<List<TierDisplayModel>> GetTierDisplayDataAsync();

    /// <summary>
    /// Get contract addresses and network information
    /// </summary>
    Task<ContractInfoModel> GetContractInfoAsync();

    /// <summary>
    /// Get investment highlights for marketing display
    /// </summary>
    List<InvestmentHighlightModel> GetInvestmentHighlights();

    /// <summary>
    /// Get fallback data when API calls fail
    /// </summary>
    HomePageDataModel GetFallbackHomeData();

    /// <summary>
    /// Check API health status
    /// </summary>
    Task<bool> CheckApiHealthAsync();
}