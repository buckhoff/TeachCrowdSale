using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Infrastructure.Repositories;
using TeachCrowdSale.Infrastructure.Repositories.Staking;
using TeachCrowdSale.Infrastructure.Services;

namespace TeachCrowdSale.Api.Extensions;

/// <summary>
/// Service collection extensions for staking functionality
/// </summary>
public static class StakingServiceExtensions
{
    /// <summary>
    /// Add staking services to the service collection
    /// </summary>
    public static IServiceCollection AddStakingServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IStakingRepository, StakingRepository>();

        // Register services
        services.AddScoped<IStakingService, StakingService>();
        services.AddScoped<IStakingContractService, StakingContractService>();

        return services;
    }

    /// <summary>
    /// Add staking rate limiting policies
    /// </summary>
    public static IServiceCollection AddStakingRateLimiting(this IServiceCollection services)
    {
        services.Configure<RateLimiterOptions>(options =>
        {
            // Staking API rate limits
            options.AddFixedWindowLimiter("Staking", limiterOptions =>
            {
                limiterOptions.PermitLimit = 50;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
            });

            // Staking transaction rate limits (more restrictive)
            options.AddFixedWindowLimiter("StakingTransaction", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 2;
            });
        });

        return services;
    }
}
