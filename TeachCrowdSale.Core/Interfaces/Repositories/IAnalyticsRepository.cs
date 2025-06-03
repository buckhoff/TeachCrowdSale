using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    // <summary>
    /// Repository interface for analytics data operations
    /// </summary>
    public interface IAnalyticsRepository
    {
        // Snapshot operations
        Task<AnalyticsSnapshot> AddSnapshotAsync(AnalyticsSnapshot snapshot);
        Task<AnalyticsSnapshot?> GetLatestSnapshotAsync();
        Task<List<AnalyticsSnapshot>> GetSnapshotsAsync(DateTime startDate, DateTime endDate);
        Task<AnalyticsSnapshot?> GetSnapshotByDateAsync(DateTime date);

        // Daily analytics operations
        Task<DailyAnalytics> AddDailyAnalyticsAsync(DailyAnalytics dailyAnalytics);
        Task<DailyAnalytics?> GetDailyAnalyticsByDateAsync(DateTime date);
        Task<List<DailyAnalytics>> GetDailyAnalyticsRangeAsync(DateTime startDate, DateTime endDate);
        Task UpdateDailyAnalyticsAsync(DailyAnalytics dailyAnalytics);

        // Performance metrics operations
        Task<PerformanceMetric> AddPerformanceMetricAsync(PerformanceMetric metric);
        Task<List<PerformanceMetric>> GetMetricsByCategoryAsync(string category, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<PerformanceMetric>> GetMetricsByNameAsync(string metricName, DateTime? startDate = null, DateTime? endDate = null);
        Task<PerformanceMetric?> GetLatestMetricAsync(string metricName);

        // Tier snapshot operations
        Task<List<TierSnapshot>> GetTierSnapshotsAsync(DateTime startDate, DateTime endDate, int? tierId = null);
        Task<TierSnapshot?> GetLatestTierSnapshotAsync(int tierId);

        // Aggregation operations
        Task<decimal> GetAverageMetricValueAsync(string metricName, DateTime startDate, DateTime endDate);
        Task<decimal> GetSumMetricValueAsync(string metricName, DateTime startDate, DateTime endDate);
        Task<(decimal min, decimal max)> GetMinMaxMetricValueAsync(string metricName, DateTime startDate, DateTime endDate);

        // Cleanup operations
        Task<int> CleanupOldDataAsync(DateTime cutoffDate);
        Task<int> ArchiveOldDataAsync(DateTime cutoffDate);
    }
}
