using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using TeachCrowdSale.Api.Service;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Infrastructure.Repositories.Liquidity;
using TeachCrowdSale.Infrastructure.Services;

namespace TeachCrowdSale.Api.Extensions
{
    /// <summary>
    /// Service collection extensions for liquidity functionality
    /// </summary>
    public static class LiquidityServiceExtensions
    {
        /// <summary>
        /// Add liquidity services to the service collection
        /// </summary>
        public static IServiceCollection AddLiquidityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register repositories
            services.AddScoped<ILiquidityRepository, LiquidityRepository>();

            // Register services
            services.AddScoped<ILiquidityService, LiquidityService>();
            services.AddScoped<IDexIntegrationService, DexIntegrationService>();

            return services;
        }

        /// <summary>
        /// Add liquidity rate limiting policies
        /// </summary>
        public static IServiceCollection AddLiquidityRateLimiting(this IServiceCollection services)
        {
            services.Configure<RateLimiterOptions>(options =>
            {
                // Liquidity API rate limits
                options.AddFixedWindowLimiter("Liquidity", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 10;
                });

                // Liquidity transaction rate limits (more restrictive)
                options.AddFixedWindowLimiter("LiquidityTransaction", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 2;
                });
            });

            return services;
        }

        /// <summary>
        /// Add liquidity caching policies
        /// </summary>
        public static IServiceCollection AddLiquidityCaching(this IServiceCollection services)
        {
            // Memory cache is already added in main service configuration
            // Additional caching policies can be configured here if needed
            return services;
        }

        /// <summary>
        /// Add liquidity background services
        /// </summary>
        public static IServiceCollection AddLiquidityBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<LiquidityDataSyncService>();
            return services;
        }
    }
}
