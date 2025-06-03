using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeachCrowdSale.Core.Interfaces.Services;
using TeachCrowdSale.Core.Models.Response;
using TeachCrowdSale.Core.Models.Treasury;

namespace TeachCrowdSale.Infrastructure.Services
{
    /// <summary>
    /// Web service for Analytics Dashboard functionality
    /// Handles HTTP client calls to Analytics API with caching and error handling
    /// </summary>
    public class AnalyticsDashboardService : IAnalyticsDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnalyticsDashboardService> _logger;

        // Cache keys
        private const string CACHE_KEY_DASHBOARD = "analytics_dashboard_data";
        private const string CACHE_KEY_TOKEN_ANALYTICS = "analytics_token_data";
        private const string CACHE_KEY_PRESALE_ANALYTICS = "analytics_presale_data";
        private const string CACHE_KEY_TREASURY_ANALYTICS = "analytics_treasury_data";
        private const string CACHE_KEY_PLATFORM_ANALYTICS = "analytics_platform_data";
        private const string CACHE_KEY_PRICE_HISTORY = "analytics_price_history";
        private const string CACHE_KEY_VOLUME_HISTORY = "analytics_volume_history";
        private const string CACHE_KEY_TIER_PERFORMANCE = "analytics_tier_performance";

        // Cache durations following home page patterns
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromMinutes(15);

        public AnalyticsDashboardService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<AnalyticsDashboardService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("TeachAPI");
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AnalyticsDashboardResponse?> GetDashboardDataAsync()
        {
            var cacheKey = CACHE_KEY_DASHBOARD;

            if (_cache.TryGetValue(cacheKey, out AnalyticsDashboardResponse? cachedData) && cachedData != null)
            {
                _logger.LogDebug("Analytics dashboard data retrieved from cache");
                return cachedData;
            }

            try
            {
                _logger.LogDebug("Fetching analytics dashboard data from API");
                var response = await _httpClient.GetAsync("/api/analytics/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<AnalyticsDashboardResponse>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _shortCacheDuration);
                        _logger.LogDebug("Analytics dashboard data cached successfully");
                        return data;
                    }
                }
                else
                {
                    _logger.LogWarning("Analytics API returned {StatusCode}: {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching analytics dashboard data from API");
            }

            // Return fallback data
            return GetFallbackDashboardData();
        }

        public async Task<TokenAnalyticsResponse?> GetTokenAnalyticsAsync()
        {
            var cacheKey = CACHE_KEY_TOKEN_ANALYTICS;

            if (_cache.TryGetValue(cacheKey, out TokenAnalyticsResponse? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var response = await _httpClient.GetAsync("/api/analytics/token");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<TokenAnalyticsResponse>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _mediumCacheDuration);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching token analytics from API");
            }

            return GetFallbackTokenAnalytics();
        }

        public async Task<PresaleAnalyticsResponse?> GetPresaleAnalyticsAsync()
        {
            var cacheKey = CACHE_KEY_PRESALE_ANALYTICS;

            if (_cache.TryGetValue(cacheKey, out PresaleAnalyticsResponse? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var response = await _httpClient.GetAsync("/api/analytics/presale");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<PresaleAnalyticsResponse>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _mediumCacheDuration);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching presale analytics from API");
            }

            return GetFallbackPresaleAnalytics();
        }

        public async Task<TreasuryAnalyticsResponse?> GetTreasuryAnalyticsAsync()
        {
            var cacheKey = CACHE_KEY_TREASURY_ANALYTICS;

            if (_cache.TryGetValue(cacheKey, out TreasuryAnalyticsResponse? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var response = await _httpClient.GetAsync("/api/analytics/treasury");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<TreasuryAnalyticsResponse>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _longCacheDuration);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching treasury analytics from API");
            }

            return GetFallbackTreasuryAnalytics();
        }

        public async Task<PlatformAnalyticsResponse?> GetPlatformAnalyticsAsync()
        {
            var cacheKey = CACHE_KEY_PLATFORM_ANALYTICS;

            if (_cache.TryGetValue(cacheKey, out PlatformAnalyticsResponse? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var response = await _httpClient.GetAsync("/api/analytics/platform");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<PlatformAnalyticsResponse>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _longCacheDuration);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching platform analytics from API");
            }

            return GetFallbackPlatformAnalytics();
        }

        public async Task<List<TimeSeriesDataPointResponse>?> GetPriceHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string interval = "1d")
        {
            var cacheKey = $"{CACHE_KEY_PRICE_HISTORY}_{startDate?.ToString("yyyyMMdd")}_{endDate?.ToString("yyyyMMdd")}_{interval}";

            if (_cache.TryGetValue(cacheKey, out List<TimeSeriesDataPointResponse>? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var url = $"/api/analytics/price-history?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}&interval={interval}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<TimeSeriesDataPointResponse>>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _mediumCacheDuration);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price history from API");
            }

            return GetFallbackPriceHistory();
        }

        public async Task<List<TimeSeriesDataPointResponse>?> GetVolumeHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string interval = "1d")
        {
            var cacheKey = $"{CACHE_KEY_VOLUME_HISTORY}_{startDate?.ToString("yyyyMMdd")}_{endDate?.ToString("yyyyMMdd")}_{interval}";

            if (_cache.TryGetValue(cacheKey, out List<TimeSeriesDataPointResponse>? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var url = $"/api/analytics/volume-history?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}&interval={interval}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<TimeSeriesDataPointResponse>>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _mediumCacheDuration);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching volume history from API");
            }

            return GetFallbackVolumeHistory();
        }

        public async Task<List<TierPerformanceResponse>?> GetTierPerformanceAsync()
        {
            var cacheKey = CACHE_KEY_TIER_PERFORMANCE;

            if (_cache.TryGetValue(cacheKey, out List<TierPerformanceResponse>? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var response = await _httpClient.GetAsync("/api/analytics/tiers");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<TierPerformanceResponse>>(content, GetJsonOptions());

                    if (data != null)
                    {
                        _cache.Set(cacheKey, data, _mediumCacheDuration);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tier performance from API");
            }

            return GetFallbackTierPerformance();
        }

        #region Fallback Data Methods

        private AnalyticsDashboardResponse GetFallbackDashboardData()
        {
            _logger.LogWarning("Using fallback analytics dashboard data");

            return new AnalyticsDashboardResponse
            {
                TokenAnalytics = GetFallbackTokenAnalytics(),
                PresaleAnalytics = GetFallbackPresaleAnalytics(),
                TreasuryAnalytics = GetFallbackTreasuryAnalytics(),
                PlatformAnalytics = GetFallbackPlatformAnalytics(),
                TierPerformance = GetFallbackTierPerformance(),
                PriceHistory = GetFallbackPriceHistory(),
                VolumeHistory = GetFallbackVolumeHistory(),
                LoadedAt = DateTime.UtcNow
            };
        }

        private TokenAnalyticsResponse GetFallbackTokenAnalytics()
        {
            return new TokenAnalyticsResponse
            {
                CurrentPrice = 0.065m,
                MarketCap = 325_000_000m,
                Volume24h = 2_500_000m,
                PriceChange24h = 4.8m,
                HoldersCount = 3247,
                TotalSupply = 5_000_000_000m,
                CirculatingSupply = 1_000_000_000m,
                BurnedTokens = 0m,
                StakedTokens = 0m,
                AllTimeHigh = 0.075m,
                AllTimeHighDate = DateTime.UtcNow.AddDays(-15),
                AllTimeLow = 0.04m,
                AllTimeLowDate = DateTime.UtcNow.AddDays(-45)
            };
        }

        private PresaleAnalyticsResponse GetFallbackPresaleAnalytics()
        {
            return new PresaleAnalyticsResponse
            {
                TotalRaised = 12_500_000m,
                FundingGoal = 87_500_000m,
                TokensSold = 750_000_000m,
                TokensRemaining = 500_000_000m,
                ParticipantsCount = 2847,
                CurrentTier = new CurrentTierAnalyticsResponse
                {
                    TierId = 2,
                    TierName = "Community Round",
                    Price = 0.06m,
                    Allocation = 375_000_000m,
                    Sold = 169_000_000m,
                    Remaining = 206_000_000m,
                    SoldToday = 125_000m,
                    SoldThisWeek = 875_000m,
                    SalesVelocity = 1_250_000m
                },
                PresaleStartDate = DateTime.UtcNow.AddDays(-60),
                WeeklyRunRate = 2_500_000m,
                MonthlyProjection = 10_750_000m
            };
        }

        private TreasuryAnalyticsResponse GetFallbackTreasuryAnalytics()
        {
            return new TreasuryAnalyticsResponse
            {
                TotalTreasuryValue = 87_500_000m,
                StabilityFundBalance = 8_750_000m,
                OperationalFunds = 26_250_000m,
                ReserveFunds = 13_125_000m,
                MonthlyBurnRate = 695_000m,
                OperationalRunwayMonths = 126,
                TotalRunwayMonths = 126,
                Overview = new TreasuryOverviewModel
                {
                    TotalValue = 87_500_000m,
                    SafetyFundValue = 8_750_000m,
                    StabilityFundValue = 8_750_000m,
                    MonthlyBurnRate = 695_000m,
                    OperationalRunwayYears = 10.5m,
                    Timestamp = DateTime.UtcNow,
                    IsLatest = true
                },
                FundAllocations = new List<FundAllocationResponse>
                {
                    new() { Category = "Development", Amount = 26_250_000m, Percentage = 30m, Status = "Allocated" },
                    new() { Category = "Marketing", Amount = 17_500_000m, Percentage = 20m, Status = "Allocated" },
                    new() { Category = "Operations", Amount = 13_125_000m, Percentage = 15m, Status = "Allocated" },
                    new() { Category = "Legal & Compliance", Amount = 8_750_000m, Percentage = 10m, Status = "Reserved" },
                    new() { Category = "Partnerships", Amount = 8_750_000m, Percentage = 10m, Status = "Reserved" },
                    new() { Category = "Reserve Fund", Amount = 13_125_000m, Percentage = 15m, Status = "Reserved" }
                }
            };
        }

        private PlatformAnalyticsResponse GetFallbackPlatformAnalytics()
        {
            return new PlatformAnalyticsResponse
            {
                TotalValueLocked = 0m,
                StakedTokens = 0m,
                RewardsDistributed = 0m,
                ActiveStakers = 0,
                TotalEducationFunding = 0m,
                EducatorsSupported = 0,
                StudentsImpacted = 0,
                SchoolsPartnered = 0,
                TransactionsToday = 45,
                ActiveUsers24h = 127,
                ActiveUsers7d = 453,
                PlatformGrowthRate = 0m,
                UserRetentionRate = 0m,
                AverageSessionDuration = 0m
            };
        }

        private List<TierPerformanceResponse> GetFallbackTierPerformance()
        {
            return new List<TierPerformanceResponse>
            {
                new()
                {
                    TierId = 1,
                    TierName = "Seed Round",
                    Price = 0.04m,
                    Allocation = 250_000_000m,
                    Sold = 250_000_000m,
                    IsActive = false,
                    IsSoldOut = true,
                    Status = "Completed",
                    RevenueGenerated = 10_000_000m,
                    SalesVelocity = 0m,
                    ParticipantsCount = 1247,
                    AverageInvestment = 8026.42m
                },
                new()
                {
                    TierId = 2,
                    TierName = "Community Round",
                    Price = 0.06m,
                    Allocation = 375_000_000m,
                    Sold = 169_000_000m,
                    IsActive = true,
                    IsSoldOut = false,
                    Status = "Active",
                    RevenueGenerated = 10_140_000m,
                    SalesVelocity = 1_250_000m,
                    SoldToday = 125_000m,
                    SoldThisWeek = 875_000m,
                    ParticipantsCount = 1600,
                    AverageInvestment = 6337.50m
                },
                new()
                {
                    TierId = 3,
                    TierName = "Growth Round",
                    Price = 0.08m,
                    Allocation = 375_000_000m,
                    Sold = 0m,
                    IsActive = false,
                    IsSoldOut = false,
                    Status = "Upcoming",
                    RevenueGenerated = 0m,
                    SalesVelocity = 0m,
                    ParticipantsCount = 0,
                    AverageInvestment = 0m
                },
                new()
                {
                    TierId = 4,
                    TierName = "Final Round",
                    Price = 0.10m,
                    Allocation = 250_000_000m,
                    Sold = 0m,
                    IsActive = false,
                    IsSoldOut = false,
                    Status = "Upcoming",
                    RevenueGenerated = 0m,
                    SalesVelocity = 0m,
                    ParticipantsCount = 0,
                    AverageInvestment = 0m
                }
            };
        }

        private List<TimeSeriesDataPointResponse> GetFallbackPriceHistory()
        {
            var data = new List<TimeSeriesDataPointResponse>();
            var baseDate = DateTime.UtcNow.AddDays(-30);

            for (int i = 0; i <= 30; i++)
            {
                var date = baseDate.AddDays(i);
                var basePrice = 0.04m + (i * 0.0008m); // Gradual price increase
                var volatility = (decimal)(Math.Sin(i * 0.2) * 0.003);

                data.Add(new TimeSeriesDataPointResponse
                {
                    Timestamp = date,
                    Value = basePrice + volatility,
                    Open = basePrice + volatility - 0.001m,
                    High = basePrice + volatility + 0.002m,
                    Low = basePrice + volatility - 0.002m,
                    Close = basePrice + volatility,
                    Volume = 50000m + (i * 1000m),
                    Category = "Price"
                });
            }

            return data;
        }

        private List<TimeSeriesDataPointResponse> GetFallbackVolumeHistory()
        {
            var data = new List<TimeSeriesDataPointResponse>();
            var baseDate = DateTime.UtcNow.AddDays(-30);

            for (int i = 0; i <= 30; i++)
            {
                var date = baseDate.AddDays(i);
                var baseVolume = 100000m + (i * 15000m);
                var dailyVariation = (decimal)(Math.Sin(i * 0.3) * 50000);

                data.Add(new TimeSeriesDataPointResponse
                {
                    Timestamp = date,
                    Value = baseVolume + dailyVariation,
                    Volume = baseVolume + dailyVariation,
                    Category = "Volume"
                });
            }

            return data;
        }

        #endregion

        #region Helper Methods

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #endregion
    }
}