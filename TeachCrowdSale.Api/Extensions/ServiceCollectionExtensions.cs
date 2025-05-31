using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;
using System.Threading.RateLimiting;
using TeachCrowdSale.Api.Converter;
using TeachCrowdSale.Api.ModelBinding;
using TeachCrowdSale.Api.Validator;
using TeachCrowdSale.Infrastructure.Configuration;
using TeachCrowdSale.Infrastructure.Data.Context;
using TeachCrowdSale.Infrastructure.Repositories;
using TeachCrowdSale.Infrastructure.Services;
using TeachCrowdSale.Infrastructure.Web3;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Interfaces.Repositories;

namespace TeachCrowdSale.Api.Extensions
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?.Replace("{DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "sa")
                ?.Replace("{DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "");

            services.AddDbContext<TeachCrowdSaleDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }

        public static IServiceCollection AddApiControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new EthereumAddressModelBinderProvider());
                options.ModelValidatorProviders.Clear();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new EthereumAddressJsonConverter());
            })
            .AddApiControllerConventions();

            return services;
        }

        /// <summary>
        /// Add all tokenomics-related services and configurations
        /// </summary>
        public static IServiceCollection AddTokenomicsModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTokenomicsServices(configuration);
            services.AddTokenomicsRateLimiting();
            services.AddTokenomicsCaching();
            services.AddTokenomicsBackgroundServices();

            return services;
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddSingleton<Web3Helper>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IBlockchainService, BlockchainService>();
            services.AddScoped<IPresaleService, PresaleService>();
            services.AddScoped<ITokenContractService, TokenContractService>();
            services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();
            services.AddScoped<IAbiService, AbiService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }

        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<PurchaseRequestValidator>();
            return services;
        }

        public static IServiceCollection AddConfigurationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BlockchainSettings>(configuration.GetSection("Blockchain"));
            services.Configure<JwtSettings>(options =>
            {
                var jwtSection = configuration.GetSection("JwtSettings");
                options.Issuer = jwtSection["Issuer"];
                options.Audience = jwtSection["Audience"];
                options.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? jwtSection["SecretKey"];
                options.ExpiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");
                options.RefreshExpiryDays = int.Parse(jwtSection["RefreshExpiryDays"] ?? "7");
            });

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });

            return services;
        }

        public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 200,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddFixedWindowLimiter("TokenInfo", options =>
                {
                    options.PermitLimit = 30;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 5;
                });

                options.AddFixedWindowLimiter("Purchase", options =>
                {
                    options.PermitLimit = 5;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                });

                options.AddFixedWindowLimiter("Auth", options =>
                {
                    options.PermitLimit = 10;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 3;
                });
                options.AddFixedWindowLimiter("BuyTrade", options =>
                {
                    options.PermitLimit = 50;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 10;
                });
    
                options.AddFixedWindowLimiter("Tokenomics", options =>
                {
                    options.PermitLimit = 100;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 10;
                });

                options.AddFixedWindowLimiter("LiveMetrics", options =>
                {
                    options.PermitLimit = 60;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 5;
                });
            });

            return services;
        }

        public static IServiceCollection AddCorsServices(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(
                        "https://localhost:5173",
                        "http://localhost:5173",
                        "https://teachtoken.io")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddCompressionServices(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            return services;
        }
    }
}
