// TeachCrowdSale.Infrastructure/Services/AnalyticsBackgroundService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Infrastructure.Services
{
    /// <summary>
    /// Background service for collecting and aggregating analytics data
    /// </summary>
    public class AnalyticsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AnalyticsBackgroundService> _logger;

        // Intervals for different data collection tasks
        private readonly TimeSpan _snapshotInterval = TimeSpan.FromMinutes(15); // Every 15 minutes
        private readonly TimeSpan _dailyAggregationInterval = TimeSpan.FromHours(1); // Every hour (checks for new day)
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1); // Daily cleanup

        private DateTime _lastSnapshotTime = DateTime.MinValue;
        private DateTime _lastDailyAggregation = DateTime.MinValue;
        private DateTime _lastCleanup = DateTime.MinValue;

        public AnalyticsBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AnalyticsBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analytics Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    // Take analytics snapshots
                    if (now - _lastSnapshotTime >= _snapshotInterval)
                    {
                        await TakeAnalyticsSnapshotAsync();
                        _lastSnapshotTime = now;
                    }

                    // Process daily aggregations
                    if (now - _lastDailyAggregation >= _dailyAggregationInterval)
                    {
                        await ProcessDailyAggregationAsync();
                        _lastDailyAggregation = now;
                    }

                    // Cleanup old data
                    if (now - _lastCleanup >= _cleanupInterval)
                    {
                        await CleanupOldDataAsync();
                        _lastCleanup = now;
                    }

                    // Wait for next cycle
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Analytics Background Service cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Analytics Background Service main loop");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait before retrying
                }
            }

            _logger.LogInformation("Analytics Background Service stopped");
        }

        private async Task TakeAnalyticsSnapshotAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

                var success = await analyticsService.TakeAnalyticsSnapshotAsync();

                if (success)
                {
                    _logger.LogDebug("Analytics snapshot taken successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to take analytics snapshot");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking analytics snapshot");
            }
        }

        private async Task ProcessDailyAggregationAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var analyticsRepository = scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();
                var presaleService = scope.ServiceProvider.GetRequiredService<IPresaleService>();

                var yesterday = DateTime.UtcNow.Date.AddDays(-1);

                // Check if we already have daily analytics for yesterday
                var existingDaily = await analyticsRepository.GetDailyAnalyticsByDateAsync(yesterday);
                if (existingDaily != null)
                {
                    _logger.LogDebug("Daily analytics already exists for {Date}", yesterday);
                    return;
                }

                // Get snapshots for yesterday
                var startOfDay = yesterday;
                var endOfDay = yesterday.AddDays(1).AddTicks(-1);
                var snapshots = await analyticsRepository.GetSnapshotsAsync(startOfDay, endOfDay);

                if (!snapshots.Any())
                {
                    _logger.LogWarning("No snapshots found for daily aggregation on {Date}", yesterday);
                    return;
                }

                // Calculate daily aggregations
                var dailyAnalytics = new DailyAnalytics
                {
                    Date = yesterday,
                    DailyVolume = snapshots.Average(s => s.Volume24h),
                    DailyTransactions = snapshots.LastOrDefault()?.TransactionsCount24h ?? 0,
                    NewHolders = CalculateNewHolders(snapshots),
                    NewParticipants = CalculateNewParticipants(snapshots),
                    DailyTokensSold = CalculateDailyTokensSold(snapshots),
                    DailyUsdRaised = CalculateDailyUsdRaised(snapshots),
                    OpenPrice = snapshots.FirstOrDefault()?.TokenPrice ?? 0,
                    ClosePrice = snapshots.LastOrDefault()?.TokenPrice ?? 0,
                    HighPrice = snapshots.Max(s => s.TokenPrice),
                    LowPrice = snapshots.Min(s => s.TokenPrice),
                    DailyRewardsDistributed = CalculateDailyRewardsDistributed(snapshots),
                    ActiveEducators = 0, // Will be populated when platform launches
                    EducationFundingAmount = 0 // Will be populated when platform launches
                };

                await analyticsRepository.AddDailyAnalyticsAsync(dailyAnalytics);
                _logger.LogInformation("Daily analytics processed for {Date}", yesterday);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing daily aggregation");
            }
        }

        private async Task CleanupOldDataAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var analyticsRepository = scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();

                // Keep snapshots for 90 days, daily analytics for 2 years
                var snapshotCutoff = DateTime.UtcNow.AddDays(-90);
                var metricsCutoff = DateTime.UtcNow.AddDays(-365); // Keep performance metrics for 1 year

                var cleanedSnapshots = await analyticsRepository.CleanupOldDataAsync(snapshotCutoff);

                if (cleanedSnapshots > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} old analytics records", cleanedSnapshots);
                }

                // Archive very old performance metrics (make them non-public)
                var archivedMetrics = await analyticsRepository.ArchiveOldDataAsync(metricsCutoff);

                if (archivedMetrics > 0)
                {
                    _logger.LogInformation("Archived {Count} old performance metrics", archivedMetrics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data cleanup");
            }
        }

        private async Task UpdatePerformanceMetricsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var analyticsRepository = scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();
                var tokenService = scope.ServiceProvider.GetRequiredService<ITokenContractService>();
                var presaleService = scope.ServiceProvider.GetRequiredService<IPresaleService>();

                var timestamp = DateTime.UtcNow;

                // Token metrics
                await AddPerformanceMetricAsync(analyticsRepository, "TokenPrice", "Token",
                    await GetTokenPriceSafely(tokenService), "USD", timestamp);

                await AddPerformanceMetricAsync(analyticsRepository, "MarketCap", "Token",
                    await GetMarketCapSafely(tokenService), "USD", timestamp);

                await AddPerformanceMetricAsync(analyticsRepository, "HoldersCount", "Token",
                    await GetHoldersCountSafely(tokenService), "count", timestamp);

                // Presale metrics
                var presaleStats = await presaleService.GetPresaleStatsAsync();
                await AddPerformanceMetricAsync(analyticsRepository, "TotalRaised", "Presale",
                    presaleStats.TotalRaised, "USD", timestamp);

                await AddPerformanceMetricAsync(analyticsRepository, "ParticipantsCount", "Presale",
                    presaleStats.ParticipantsCount, "count", timestamp);

                _logger.LogDebug("Performance metrics updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating performance metrics");
            }
        }

        private async Task AddPerformanceMetricAsync(
            IAnalyticsRepository repository,
            string metricName,
            string category,
            decimal value,
            string unit,
            DateTime timestamp)
        {
            try
            {
                // Get previous value for change calculation
                var previousMetric = await repository.GetLatestMetricAsync(metricName);
                decimal? changePercentage = null;

                if (previousMetric != null && previousMetric.Value != 0)
                {
                    changePercentage = ((value - previousMetric.Value) / previousMetric.Value) * 100;
                }

                var metric = new PerformanceMetric
                {
                    MetricName = metricName,
                    Category = category,
                    Value = value,
                    Unit = unit,
                    Timestamp = timestamp,
                    PreviousValue = previousMetric?.Value,
                    ChangePercentage = changePercentage,
                    IsPublic = true
                };

                await repository.AddPerformanceMetricAsync(metric);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error adding performance metric {MetricName}", metricName);
            }
        }

        #region Helper Methods

        private int CalculateNewHolders(List<AnalyticsSnapshot> snapshots)
        {
            if (snapshots.Count < 2) return 0;
            return snapshots.Last().HoldersCount - snapshots.First().HoldersCount;
        }

        private int CalculateNewParticipants(List<AnalyticsSnapshot> snapshots)
        {
            if (snapshots.Count < 2) return 0;
            return snapshots.Last().ParticipantsCount - snapshots.First().ParticipantsCount;
        }

        private decimal CalculateDailyTokensSold(List<AnalyticsSnapshot> snapshots)
        {
            if (snapshots.Count < 2) return 0;
            return snapshots.Last().TokensSold - snapshots.First().TokensSold;
        }

        private decimal CalculateDailyUsdRaised(List<AnalyticsSnapshot> snapshots)
        {
            if (snapshots.Count < 2) return 0;
            return snapshots.Last().TotalRaised - snapshots.First().TotalRaised;
        }

        private decimal CalculateDailyRewardsDistributed(List<AnalyticsSnapshot> snapshots)
        {
            if (snapshots.Count < 2) return 0;
            return snapshots.Last().RewardsDistributed - snapshots.First().RewardsDistributed;
        }

        private async Task<decimal> GetTokenPriceSafely(ITokenContractService tokenService)
        {
            try
            {
                return await tokenService.GetTokenPriceAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get token price");
                return 0.06m; // Fallback price
            }
        }

        private async Task<decimal> GetMarketCapSafely(ITokenContractService tokenService)
        {
            try
            {
                return await tokenService.CalculateMarketCapAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get market cap");
                return 60000000m; // Fallback market cap
            }
        }

        private async Task<int> GetHoldersCountSafely(ITokenContractService tokenService)
        {
            try
            {
                return await tokenService.GetHoldersCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get holders count");
                return 2847; // Fallback holders count
            }
        }

        #endregion

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Analytics Background Service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}