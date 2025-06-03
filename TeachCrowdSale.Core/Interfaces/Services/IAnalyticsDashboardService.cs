using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models.Treasury;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for Analytics Dashboard web service
    /// </summary>
    public interface IAnalyticsDashboardService
    {
        /// <summary>
        /// Get comprehensive dashboard data
        /// </summary>
        Task<AnalyticsDashboardResponse?> GetDashboardDataAsync();

        /// <summary>
        /// Get token-specific analytics
        /// </summary>
        Task<TokenAnalyticsResponse?> GetTokenAnalyticsAsync();

        /// <summary>
        /// Get presale-specific analytics
        /// </summary>
        Task<PresaleAnalyticsResponse?> GetPresaleAnalyticsAsync();

        /// <summary>
        /// Get treasury-specific analytics
        /// </summary>
        Task<TreasuryAnalyticsResponse?> GetTreasuryAnalyticsAsync();

        /// <summary>
        /// Get platform-specific analytics
        /// </summary>
        Task<PlatformAnalyticsResponse?> GetPlatformAnalyticsAsync();

        /// <summary>
        /// Get price history for charts
        /// </summary>
        Task<List<TimeSeriesDataPointResponse>?> GetPriceHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string interval = "1d");

        /// <summary>
        /// Get volume history for charts
        /// </summary>
        Task<List<TimeSeriesDataPointResponse>?> GetVolumeHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string interval = "1d");

        /// <summary>
        /// Get tier performance analytics
        /// </summary>
        Task<List<TierPerformanceResponse>?> GetTierPerformanceAsync();

        
        Task<object> GetAnalyticsSummaryAsync();
    }
}