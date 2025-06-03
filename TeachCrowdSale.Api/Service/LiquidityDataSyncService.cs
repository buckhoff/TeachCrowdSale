using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Api.Service
{
    /// <summary>
    /// Background service for syncing liquidity data from DEX APIs
    /// </summary>
    public class LiquidityDataSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LiquidityDataSyncService> _logger;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5); // Sync every 5 minutes
        private readonly TimeSpan _priceRefreshInterval = TimeSpan.FromMinutes(1); // Refresh prices every minute

        public LiquidityDataSyncService(
            IServiceProvider serviceProvider,
            ILogger<LiquidityDataSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait a bit before starting to allow other services to initialize
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            var lastFullSync = DateTime.MinValue;
            var lastPriceRefresh = DateTime.MinValue;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    using var scope = _serviceProvider.CreateScope();
                    var liquidityService = scope.ServiceProvider.GetRequiredService<ILiquidityService>();

                    // Refresh prices more frequently
                    if (now - lastPriceRefresh >= _priceRefreshInterval)
                    {
                        _logger.LogInformation("Starting price refresh");
                        await liquidityService.RefreshPoolPricesAsync();
                        lastPriceRefresh = now;
                        _logger.LogInformation("Completed price refresh");
                    }

                    // Full data sync less frequently
                    if (now - lastFullSync >= _syncInterval)
                    {
                        _logger.LogInformation("Starting full liquidity data sync");
                        await liquidityService.SyncAllPoolsDataAsync();
                        lastFullSync = now;
                        _logger.LogInformation("Completed full liquidity data sync");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during liquidity data sync");
                }

                // Wait before next iteration
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
