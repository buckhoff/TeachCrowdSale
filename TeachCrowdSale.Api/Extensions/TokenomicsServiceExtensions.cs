using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Infrastructure.Data.Context;
using TeachCrowdSale.Infrastructure.Repositories.Tokenomics;
using TeachCrowdSale.Infrastructure.Services;
using Microsoft.AspNetCore.RateLimiting;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Api.Service;

namespace TeachCrowdSale.Api.Extensions
{
    public static class TokenomicsServiceExtensions
    {
        /// <summary>
        /// Add tokenomics services to the DI container
        /// </summary>
        public static IServiceCollection AddTokenomicsServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add tokenomics database context
            services.AddTokenomicsDatabase(configuration);

            // Add tokenomics repositories
            services.AddTokenomicsRepositories();

            // Add tokenomics services
            services.AddScoped<ITokenomicsService, EnhancedTokenomicsService>();

            return services;
        }

        /// <summary>
        /// Add tokenomics database context
        /// </summary>
        public static IServiceCollection AddTokenomicsDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("TokenomicsConnection")
                ?? configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No connection string found for tokenomics database");
            }

            // Replace environment variables in connection string
            connectionString = connectionString
                ?.Replace("{DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "sa")
                ?.Replace("{DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "");

            services.AddDbContext<TeachCrowdSaleDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    sqlOptions.CommandTimeout(60);
                });

                // Enable sensitive data logging in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            return services;
        }

        /// <summary>
        /// Add tokenomics repositories
        /// </summary>
        public static IServiceCollection AddTokenomicsRepositories(this IServiceCollection services)
        {
            services.AddScoped<ITokenMetricsRepository, TokenMetricsRepository>();
            services.AddScoped<ISupplyRepository, SupplyRepository>();
            services.AddScoped<IVestingRepository, VestingRepository>();
            services.AddScoped<ITreasuryRepository, TreasuryRepository>();
            services.AddScoped<IBurnRepository, BurnRepository>();
            services.AddScoped<IGovernanceRepository, GovernanceRepository>();

            return services;
        }

        /// <summary>
        /// Add tokenomics rate limiting policies
        /// </summary>
        public static IServiceCollection AddTokenomicsRateLimiting(this IServiceCollection services)
        {
            services.Configure<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>(options =>
            {
                // Tokenomics general endpoints
                options.AddFixedWindowLimiter("Tokenomics", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 10;
                });

                // Live metrics - more frequent updates allowed
                options.AddFixedWindowLimiter("LiveMetrics", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 60;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 5;
                });
            });

            return services;
        }

        /// <summary>
        /// Configure tokenomics caching policies
        /// </summary>
        public static IServiceCollection AddTokenomicsCaching(this IServiceCollection services)
        {
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1000; // Limit cache size
                options.CompactionPercentage = 0.25; // Remove 25% when limit reached
            });

            // Add distributed cache if needed for multi-instance deployments
            services.AddStackExchangeRedisCache(options =>
            {
                var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
                if (!string.IsNullOrEmpty(redisConnection))
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "TeachTokenomics";
                }
            });

            return services;
        }

        /// <summary>
        /// Configure tokenomics background services
        /// </summary>
        public static IServiceCollection AddTokenomicsBackgroundServices(this IServiceCollection services)
        {
            // Add background service for periodic data updates
            services.AddHostedService<TokenomicsDataUpdateService>();

            // Add background service for metrics collection
            services.AddHostedService<TokenomicsMetricsCollectorService>();

            return services;
        }
    }

    /// <summary>
    /// Background service for periodic tokenomics data updates
    /// </summary>
    public class TokenomicsDataUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenomicsDataUpdateService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);

        public TokenomicsDataUpdateService(
            IServiceProvider serviceProvider,
            ILogger<TokenomicsDataUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var tokenomicsService = scope.ServiceProvider.GetRequiredService<ITokenomicsService>();

                    _logger.LogInformation("Starting periodic tokenomics data update");

                    // Update live metrics
                    await tokenomicsService.GetLiveTokenMetricsAsync();

                    // Update supply data less frequently
                    if (DateTime.UtcNow.Minute % 15 == 0) // Every 15 minutes
                    {
                        await tokenomicsService.GetSupplyBreakdownAsync();
                        await tokenomicsService.GetBurnMechanicsAsync();
                        await tokenomicsService.GetTreasuryAnalyticsAsync();
                    }

                    _logger.LogInformation("Completed periodic tokenomics data update");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during periodic tokenomics data update");
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }
        }
    }
}
