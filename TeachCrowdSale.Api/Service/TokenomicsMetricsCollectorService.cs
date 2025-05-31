using TeachCrowdSale.Core.Interfaces.Services;

namespace TeachCrowdSale.Api.Service
{
    /// <summary>
    /// Background service for collecting and storing tokenomics metrics
    /// </summary>
    public class TokenomicsMetricsCollectorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenomicsMetricsCollectorService> _logger;
        private readonly TimeSpan _collectionInterval = TimeSpan.FromMinutes(10);

        public TokenomicsMetricsCollectorService(
            IServiceProvider serviceProvider,
            ILogger<TokenomicsMetricsCollectorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait a bit before starting to allow other services to initialize
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var tokenomicsService = scope.ServiceProvider.GetRequiredService<ITokenomicsService>();

                    _logger.LogInformation("Starting tokenomics metrics collection");

                    // Collect and store metrics snapshots
                    var liveMetrics = await tokenomicsService.GetLiveTokenMetricsAsync();

                    // Store historical data points for charts and analysis
                    // This would involve saving to time-series tables or external monitoring systems

                    _logger.LogInformation("Completed tokenomics metrics collection");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during tokenomics metrics collection");
                }

                await Task.Delay(_collectionInterval, stoppingToken);
            }
        }
    }
}
