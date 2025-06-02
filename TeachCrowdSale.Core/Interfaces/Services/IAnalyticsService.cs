using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models.Treasury;

namespace TeachCrowdSale.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for analytics operations
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Get comprehensive analytics dashboard data
        /// </summary>
        Task<AnalyticsDashboardResponse> GetAnalyticsDashboardAsync();

        /// <summary>
        /// Get token-specific analytics
        /// </summary>
        Task<TokenAnalyticsResponse> GetTokenAnalyticsAsync();

        /// <summary>
        /// Get presale-specific analytics
        /// </summary>
        Task<PresaleAnalyticsResponse> GetPresaleAnalyticsAsync();

        /// <summary>
        /// Get platform-specific analytics
        /// </summary>
        Task<PlatformAnalyticsResponse> GetPlatformAnalyticsAsync();

        /// <summary>
        /// Get treasury-specific analytics
        /// </summary>
        Task<TreasuryAnalyticsResponse> GetTreasuryAnalyticsAsync();

        /// <summary>
        /// Get tier performance analytics
        /// </summary>
        Task<List<TierPerformanceResponse>> GetTierPerformanceAsync();

        /// <summary>
        /// Get price history for charts
        /// </summary>
        Task<List<TimeSeriesDataPointResponse>> GetPriceHistoryAsync(DateTime startDate, DateTime endDate, string interval = "1d");

        /// <summary>
        /// Get volume history for charts
        /// </summary>
        Task<List<TimeSeriesDataPointResponse>> GetVolumeHistoryAsync(DateTime startDate, DateTime endDate, string interval = "1d");

        /// <summary>
        /// Get specific analytics metrics by category
        /// </summary>
        Task<List<AnalyticsMetricResponse>> GetMetricsByCategoryAsync(string category);

        /// <summary>
        /// Get real-time analytics snapshot
        /// </summary>
        Task<AnalyticsSnapshot> GetRealTimeSnapshotAsync();

        /// <summary>
        /// Take and store current analytics snapshot
        /// </summary>
        Task<bool> TakeAnalyticsSnapshotAsync();

        /// <summary>
        /// Get analytics for specific date range
        /// </summary>
        Task<List<AnalyticsSnapshot>> GetAnalyticsHistoryAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get daily aggregated analytics
        /// </summary>
        Task<List<DailyAnalytics>> GetDailyAnalyticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get analytics comparison between two periods
        /// </summary>
        Task<AnalyticsComparisonResponse> GetAnalyticsComparisonAsync(DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End);
    }
}
