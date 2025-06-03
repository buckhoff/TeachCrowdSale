// TeachCrowdSale.Infrastructure/Repositories/AnalyticsRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBitcoin.Secp256k1;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Infrastructure.Data.Context;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Infrastructure.Repositories.Analytics
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly TeachCrowdSaleDbContext _context;
        private readonly ILogger<AnalyticsRepository> _logger;

        public AnalyticsRepository(TeachCrowdSaleDbContext context, ILogger<AnalyticsRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Snapshot Operations

        public async Task<AnalyticsSnapshot> AddSnapshotAsync(AnalyticsSnapshot snapshot)
        {
            try
            {
                _context.AnalyticsSnapshots.Add(snapshot);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Analytics snapshot added for timestamp {Timestamp}", snapshot.Timestamp);
                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding analytics snapshot");
                throw;
            }
        }

        public async Task<AnalyticsSnapshot?> GetLatestSnapshotAsync()
        {
            try
            {
                return await _context.AnalyticsSnapshots
                    .Include(a => a.TierSnapshots)
                    .OrderByDescending(a => a.Timestamp)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest analytics snapshot");
                throw;
            }
        }

        public async Task<List<AnalyticsSnapshot>> GetSnapshotsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.AnalyticsSnapshots
                    .Include(a => a.TierSnapshots)
                    .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                    .OrderBy(a => a.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics snapshots for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<AnalyticsSnapshot?> GetSnapshotByDateAsync(DateTime date)
        {
            try
            {
                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                return await _context.AnalyticsSnapshots
                    .Include(a => a.TierSnapshots)
                    .Where(a => a.Timestamp >= startOfDay && a.Timestamp <= endOfDay)
                    .OrderByDescending(a => a.Timestamp)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics snapshot for date {Date}", date);
                throw;
            }
        }

        #endregion

        #region Daily Analytics Operations

        public async Task<DailyAnalytics> AddDailyAnalyticsAsync(DailyAnalytics dailyAnalytics)
        {
            try
            {
                _context.DailyAnalytics.Add(dailyAnalytics);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Daily analytics added for date {Date}", dailyAnalytics.Date);
                return dailyAnalytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding daily analytics for date {Date}", dailyAnalytics.Date);
                throw;
            }
        }

        public async Task<DailyAnalytics?> GetDailyAnalyticsByDateAsync(DateTime date)
        {
            try
            {
                var dateOnly = date.Date;
                return await _context.DailyAnalytics
                    .FirstOrDefaultAsync(d => d.Date == dateOnly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily analytics for date {Date}", date);
                throw;
            }
        }

        public async Task<List<DailyAnalytics>> GetDailyAnalyticsRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateOnly = startDate.Date;
                var endDateOnly = endDate.Date;

                return await _context.DailyAnalytics
                    .Where(d => d.Date >= startDateOnly && d.Date <= endDateOnly)
                    .OrderBy(d => d.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily analytics range from {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task UpdateDailyAnalyticsAsync(DailyAnalytics dailyAnalytics)
        {
            try
            {
                _context.DailyAnalytics.Update(dailyAnalytics);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Daily analytics updated for date {Date}", dailyAnalytics.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating daily analytics for date {Date}", dailyAnalytics.Date);
                throw;
            }
        }

        #endregion

        #region Performance Metrics Operations

        public async Task<PerformanceMetric> AddPerformanceMetricAsync(PerformanceMetric metric)
        {
            try
            {
                _context.PerformanceMetrics.Add(metric);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Performance metric {MetricName} added", metric.MetricName);
                return metric;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding performance metric {MetricName}", metric.MetricName);
                throw;
            }
        }

        public async Task<List<PerformanceMetric>> GetMetricsByCategoryAsync(string category, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.PerformanceMetrics
                    .Where(m => m.Category == category && m.IsPublic);

                if (startDate.HasValue)
                    query = query.Where(m => m.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(m => m.Timestamp <= endDate.Value);

                return await query
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics by category {Category}", category);
                throw;
            }
        }

        public async Task<List<PerformanceMetric>> GetMetricsByNameAsync(string metricName, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.PerformanceMetrics
                    .Where(m => m.MetricName == metricName && m.IsPublic);

                if (startDate.HasValue)
                    query = query.Where(m => m.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(m => m.Timestamp <= endDate.Value);

                return await query
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics by name {MetricName}", metricName);
                throw;
            }
        }

        public async Task<PerformanceMetric?> GetLatestMetricAsync(string metricName)
        {
            try
            {
                return await _context.PerformanceMetrics
                    .Where(m => m.MetricName == metricName && m.IsPublic)
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest metric {MetricName}", metricName);
                throw;
            }
        }

        #endregion

        #region Tier Snapshot Operations

        public async Task<List<TierSnapshot>> GetTierSnapshotsAsync(DateTime startDate, DateTime endDate, int? tierId = null)
        {
            try
            {
                var query = _context.TierSnapshots
                    .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate);

                if (tierId.HasValue)
                    query = query.Where(t => t.TierId == tierId.Value);

                return await query
                    .OrderBy(t => t.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tier snapshots for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<TierSnapshot?> GetLatestTierSnapshotAsync(int tierId)
        {
            try
            {
                return await _context.TierSnapshots
                    .Where(t => t.TierId == tierId)
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest tier snapshot for tier {TierId}", tierId);
                throw;
            }
        }

        #endregion

        #region Aggregation Operations

        public async Task<decimal> GetAverageMetricValueAsync(string metricName, DateTime startDate, DateTime endDate)
        {
            try
            {
                var metrics = await _context.PerformanceMetrics
                    .Where(m => m.MetricName == metricName &&
                               m.Timestamp >= startDate &&
                               m.Timestamp <= endDate &&
                               m.IsPublic)
                    .Select(m => m.Value)
                    .ToListAsync();

                return metrics.Any() ? metrics.Average() : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average for metric {MetricName}", metricName);
                throw;
            }
        }

        public async Task<decimal> GetSumMetricValueAsync(string metricName, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.PerformanceMetrics
                    .Where(m => m.MetricName == metricName &&
                               m.Timestamp >= startDate &&
                               m.Timestamp <= endDate &&
                               m.IsPublic)
                    .SumAsync(m => m.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating sum for metric {MetricName}", metricName);
                throw;
            }
        }

        public async Task<(decimal min, decimal max)> GetMinMaxMetricValueAsync(string metricName, DateTime startDate, DateTime endDate)
        {
            try
            {
                var metrics = await _context.PerformanceMetrics
                    .Where(m => m.MetricName == metricName &&
                               m.Timestamp >= startDate &&
                               m.Timestamp <= endDate &&
                               m.IsPublic)
                    .Select(m => m.Value)
                    .ToListAsync();

                if (!metrics.Any())
                    return (0, 0);

                return (metrics.Min(), metrics.Max());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating min/max for metric {MetricName}", metricName);
                throw;
            }
        }

        #endregion

        #region Cleanup Operations

        public async Task<int> CleanupOldDataAsync(DateTime cutoffDate)
        {
            try
            {
                var deletedSnapshots = await _context.AnalyticsSnapshots
                .Where(a => a.Timestamp < cutoffDate)
                    .ExecuteDeleteAsync();

                var deletedMetrics = await _context.PerformanceMetrics
                    .Where(m => m.Timestamp < cutoffDate)
                    .ExecuteDeleteAsync();

                var deletedDaily = await _context.DailyAnalytics
                    .Where(d => d.Date < cutoffDate.Date)
                    .ExecuteDeleteAsync();

                var totalDeleted = deletedSnapshots + deletedMetrics + deletedDaily;

                _logger.LogInformation("Cleaned up {TotalDeleted} old analytics records before {CutoffDate}",
                    totalDeleted, cutoffDate);

                return totalDeleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old analytics data");
                throw;
            }
        }

        public async Task<int> ArchiveOldDataAsync(DateTime cutoffDate)
        {
            try
            {
                // In a real implementation, this would move data to archive tables
                // For now, we'll just mark data as archived
                var archivedCount = await _context.PerformanceMetrics
                    .Where(m => m.Timestamp < cutoffDate)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.IsPublic, false));

                _logger.LogInformation("Archived {ArchivedCount} analytics records before {CutoffDate}",
                    archivedCount, cutoffDate);

                return archivedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving old analytics data");
                throw;
            }
        }

        #endregion
    }
}